using AssistantCore.Companion.Security;
using AssistantCore.Workers;

namespace AssistantCore.Tools.Impl;

public static class DummyTools
{
    public enum TemperatureUnit
    {
        Celsius,
        Fahrenheit
    }
    
    [LlmTool("get_weather", "Get the current weather for a given location.", 
        LlmSpeciality.All, RiskLevel.None)]
    public static string GetWeather(
        [LlmToolParam("location", "The City to get the weather for")] string location,
        [LlmToolParam("unit", "The unit of temperature (Celsius or Fahrenheit)")] TemperatureUnit unit = TemperatureUnit.Celsius)
    {
        return $"The current weather in {location} is 25° {unit}.";
    }

    [LlmTool("delete_all_data", "Deletes all user data. Use with caution!",
        LlmSpeciality.All, RiskLevel.Critical)]
    public static string DeleteAllData()
    {
        return "All user data has been deleted.";
    }
}