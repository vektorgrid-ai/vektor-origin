// D:/Github/vektor-origin/AssistantCore/Tools/ToolExecutor.cs

using System.Reflection;
using System.Text.Json;
using AssistantCore.Companion;
using AssistantCore.Companion.Dto;
using AssistantCore.Companion.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AssistantCore.Tools;

public class ToolExecutor(
    ToolCollector collector, 
    IServiceProvider services, 
    CompanionManager manager,
    ILogger<ToolExecutor> logger)
{
    public static RiskLevel MaximumAutoApproveLevel = RiskLevel.Medium;
    
    public async Task<object?> ExecuteAsync(string toolName, JsonElement args)
    {
        var tool = collector.GetTools().FirstOrDefault(t => t.Attribute.ToolName == toolName);
        if (tool.Method == null)
        {
            logger.LogWarning("Attempted to execute unknown tool: {ToolName}", toolName);
            return "Error: Tool not found.";
        }

        if (tool.Attribute.RiskLevel > MaximumAutoApproveLevel)
        {
            logger.LogWarning("Tool {ToolName} requires manual approval due to its danger level ({DangerLevel}).", toolName, tool.Attribute.RiskLevel);
            ToolApprovalRequest.ToolData data = new ToolApprovalRequest.ToolData
            {
                Name = tool.Attribute.ToolName,
                Description = tool.Attribute.Description,
                RiskLevel = RiskLevel.Critical,
            };
            manager.RequestToolApproval(data);
            // TODO: track pending approvals and return result when approved/rejected
            return $"Tool '{toolName}' requires approval before it can be executed. Approval requests have been sent to your approved devices.";
        }

        try
        {
            // Create instance of the tool class (supports DI)
            var instance = tool.Method.IsStatic 
                ? null 
                : ActivatorUtilities.GetServiceOrCreateInstance(services, tool.Method.DeclaringType!);
            
            var parameters = MapParameters(tool.Method, args);
            
            var result = tool.Method.Invoke(instance, parameters);

            if (result is Task task)
            {
                await task.ConfigureAwait(false);
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute tool {ToolName}", toolName);
            return $"Error executing tool: {ex.Message}";
        }
    }

    private object?[] MapParameters(MethodInfo method, JsonElement args)
    {
        var parameters = method.GetParameters();
        var result = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var attr = p.GetCustomAttribute<LlmToolParamAttribute>();
            var name = attr?.ParamName ?? p.Name!;

            if (args.TryGetProperty(name, out var jsonValue))
            {
                if (p.ParameterType.IsEnum)
                {
                    var enumString = jsonValue.GetString();
                    if (enumString == null || !Enum.TryParse(p.ParameterType, enumString, true, out var enumValue))
                    {
                        throw new ArgumentException($"Invalid value for enum parameter '{name}': {enumString}");
                    }
                    result[i] = enumValue;
                    continue;
                }
                result[i] = JsonSerializer.Deserialize(jsonValue.GetRawText(), p.ParameterType);
            }
            else if (p.HasDefaultValue)
            {
                result[i] = p.DefaultValue;
            }
            else
            {
                throw new ArgumentException($"Missing required parameter '{name}' for tool '{method.Name}'");
            }
        }

        return result;
    }
}