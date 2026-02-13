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
    
    public async Task<object?> ExecuteAsync(string toolName, string jsonArgs)
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

            var argsDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonArgs);
            var parameters = MapParameters(tool.Method, argsDictionary ?? []);
            
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

    private object?[] MapParameters(MethodInfo method, Dictionary<string, object> args)
    {
        var parameters = method.GetParameters();
        var result = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var attr = p.GetCustomAttribute<LlmToolParamAttribute>();
            var name = attr?.ParamName ?? p.Name!;

            if (args.TryGetValue(name, out var val))
            {
                // Simple conversion - in production you might need more robust JSON element conversion
                result[i] = Convert.ChangeType(val.ToString(), p.ParameterType);
            }
        }

        return result;
    }
}