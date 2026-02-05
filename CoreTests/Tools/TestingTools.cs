using AssistantCore.Tools;
using AssistantCore.Workers;

namespace CoreTests.Tools;

internal enum SampleEnum
{
    OptionA,
    OptionB,
    OptionC
}

internal class TestingTools
{
    [LlmTool("dummy1", "A dummy tool for testing purposes.", LlmSpeciality.General)]
    public static string DummyTool1(
        [LlmToolParam("param1", "The first parameter.", true)] string param1,
        [LlmToolParam("param2", "The second parameter.", false)] int param2 = 42)
    {
        return $"DummyTool1 called with param1: {param1}, param2: {param2}";
    }
    
    [LlmTool("dummy2", "Another dummy tool for testing.", LlmSpeciality.Coding)]
    public static int DummyTool2(
        [LlmToolParam("number", "A number to process.", true)] int number)
    {
        return number * number;
    }

    [LlmTool("enum", "A tool that takes an enum parameter.", LlmSpeciality.General)]
    public static string EnumTool(
        [LlmToolParam("option", "An option from the enum", true)]
        SampleEnum @enum)
    {
        return @enum.ToString();
    }
}