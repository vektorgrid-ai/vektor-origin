using System.Reflection;
using AssistantCore.Workers;
using Microsoft.Extensions.Logging;

namespace AssistantCore.Tools;

public class ToolCollector(ILogger<ToolCollector> logger)
{
    private bool isInitialized = false;
    private ToolDefinition[] toolMethods = [];
    private List<Assembly> assemblies = [];

    private void GetToolMethods()
    {
        logger.LogInformation("Discovering tool methods in {AssemblyCount} assemblies", assemblies.Count);
        var tools = assemblies.SelectMany(assembly => assembly.GetTypes())
            .SelectMany(t => t.GetMethods())
            .Select(m => new {Method = m, Attribute = m.GetCustomAttribute<LlmToolAttribute>()})
            .Where(t => t.Attribute != null)
            .ToArray();

        toolMethods = tools
            .Select(t => new ToolDefinition
            {
                Method = t.Method,
                Attribute = t.Attribute!,
                Params = t.Method.GetParameters()
                    .Select(p => new ToolDefinition.ParamData
                    {
                        Info = p,
                        Attribute = p.GetCustomAttribute<LlmToolParamAttribute>() ?? 
                                    throw new InvalidOperationException("Every tool parameter must have a LlmToolParamAttribute.")
                    })
                    .ToArray()
            })
            .ToArray();

        logger?.LogInformation("Discovered {ToolCount} tools", toolMethods.Length);
        isInitialized = true;
    }

    public ToolDefinition[] GetTools(bool refresh = false)
    {
        if (!isInitialized || refresh)
            GetToolMethods();
        else 
            logger?.LogInformation("Using cached tool methods; count={ToolCount}", toolMethods.Length);

        return toolMethods;
    }
    public ToolDefinition[] GetToolsBySpeciality(LlmSpeciality speciality, bool refresh = false)
    {
        if (!isInitialized || refresh) 
            GetToolMethods();
        else 
            logger?.LogInformation("Using cached tool methods; count={ToolCount}", toolMethods.Length);

        return toolMethods
            .Where(t => t.Attribute.Speciality.HasFlag(speciality))
            .ToArray();
    }
    
    public void AddAssembly(Assembly assembly)
    {
        assemblies.Add(assembly);
        logger?.LogInformation("Added assembly {AssemblyName} to ToolCollector", assembly.FullName);
    }
    public void RemoveAssembly(Assembly assembly)
    {
        assemblies.Remove(assembly);
        logger?.LogInformation("Removed assembly {AssemblyName} from ToolCollector", assembly.FullName);
    }
    public void SetAssemblies(List<Assembly> assemblyList)
    {
        assemblies = assemblyList;
        logger?.LogInformation("Set assemblies for ToolCollector; count={Count}", assemblyList.Count);
    }
}