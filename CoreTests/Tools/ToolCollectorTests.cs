using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using AssistantCore.Tools;
using AssistantCore.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreTests.Tools;

public class ToolCollectorTests
{
    private static ToolCollector CreateCollectorWithAssemblies(params Assembly[] assemblies)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());

        // Register ToolCollector as a singleton and initialize with provided assemblies and logger
        services.AddSingleton(provider =>
        {
            var logger = provider.GetService<ILogger<ToolCollector>>();
            var tc = new ToolCollector(logger!);
            tc.SetAssemblies(assemblies.ToList());
            return tc;
        });

        var sp = services.BuildServiceProvider();
        return sp.GetRequiredService<ToolCollector>();
    }

    [Fact]
    public void TestAssemblies_AddRemoveAndSetWorks()
    {
        var assembly1 = typeof(TestingTools).Assembly;
        var assembly2 = typeof(ToolCollector).Assembly;

        var collector = CreateCollectorWithAssemblies();

        collector.AddAssembly(assembly1);
        collector.AddAssembly(assembly2);

        Assert.Contains(collector.GetTools(true), t => t.Method.Name == nameof(TestingTools.DummyTool1));
        Assert.Contains(collector.GetTools(true), t => t.Method.Name == nameof(TestingTools.DummyTool2));

        collector.RemoveAssembly(assembly1);

        Assert.DoesNotContain(collector.GetTools(true), t => t.Method.Name == nameof(TestingTools.DummyTool1));
        Assert.DoesNotContain(collector.GetTools(true), t => t.Method.Name == nameof(TestingTools.DummyTool2));

        collector.SetAssemblies(new List<Assembly> { assembly1 });

        Assert.Contains(collector.GetTools(true), t => t.Method.Name == nameof(TestingTools.DummyTool1));
        Assert.Contains(collector.GetTools(true), t => t.Method.Name == nameof(TestingTools.DummyTool2));
    }
    
    [Fact]
    public void GetTools_ReturnsAllMarkedMethods()
    {
        var collector = CreateCollectorWithAssemblies(typeof(ToolCollectorTests).Assembly);

        var tools = collector.GetTools();

        Assert.Contains(tools, t => t.Method.Name == nameof(TestingTools.DummyTool1));
        Assert.Contains(tools, t => t.Method.Name == nameof(TestingTools.DummyTool2));
    }

    [Fact]
    public void GetToolsBySpeciality_FiltersCorrectly()
    {
        var collector = CreateCollectorWithAssemblies(typeof(ToolCollectorTests).Assembly);

        var generalTools = collector.GetToolsBySpeciality(LlmSpeciality.General);
        Assert.Contains(generalTools, t => t.Method.Name == nameof(TestingTools.DummyTool1));
        Assert.DoesNotContain(generalTools, t => t.Method.Name == nameof(TestingTools.DummyTool2));
    }
    
    [Fact]
    public void GetTools_ToolParametersHaveCorrectAttributes()
    {
        var collector = CreateCollectorWithAssemblies(typeof(ToolCollectorTests).Assembly);

        var tools = collector.GetTools();
        var dummy1 = tools.First(t => t.Method.Name == nameof(TestingTools.DummyTool1));

        var param1 = dummy1.Params.First(p => p.Info.Name == "param1");
        Assert.Equal("param1", param1.Attribute.ParamName);
        Assert.Equal("The first parameter.", param1.Attribute.Description);
        Assert.True(param1.Attribute.Required);

        var param2 = dummy1.Params.First(p => p.Info.Name == "param2");
        Assert.Equal("param2", param2.Attribute.ParamName);
        Assert.Equal("The second parameter.", param2.Attribute.Description);
        Assert.False(param2.Attribute.Required);

        var dummy2 = tools.First(t => t.Method.Name == nameof(TestingTools.DummyTool2));
        var numberParam = dummy2.Params.First(p => p.Info.Name == "number");
        Assert.Equal("number", numberParam.Attribute.ParamName);
        Assert.Equal("A number to process.", numberParam.Attribute.Description);
        Assert.True(numberParam.Attribute.Required);
    }

    [Fact]
    public void GetTools_EnumTool_ToolParameterHasCorrectType()
    {
        var collector = CreateCollectorWithAssemblies(typeof(ToolCollectorTests).Assembly);

        var tools = collector.GetTools();
        var enumTool = tools.First(t => t.Method.Name == nameof(TestingTools.EnumTool));

        var enumParam = enumTool.Params.Single();
        Assert.Equal("option", enumParam.Attribute.ParamName);
        Assert.Equal(typeof(SampleEnum), enumParam.Info.ParameterType);
    }
}