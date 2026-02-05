using System.Collections.Concurrent;

namespace AssistantCore.Workers;

public static class LlmInfo
{
    private static ConcurrentDictionary<LlmSpeciality, (string system, string model)> _llmConfigs = new();
    public static void ParseConfig(IConfigurationSection configSection, ILogger logger)
    {
        foreach (var llmConfig in configSection.GetChildren())
        {
            var specialityStr = llmConfig.Key;
            logger.LogInformation("Parsing LLM config for speciality '{Speciality}'", specialityStr);
            if (!Enum.TryParse<LlmSpeciality>(specialityStr, ignoreCase: true, out var speciality))
            {
                logger.LogWarning("Unknown LlmSpeciality in configuration: '{Key}'", specialityStr);
                continue;
            }

            var systemPrompt = llmConfig.GetValue<string>("SystemPrompt");
            var model = llmConfig.GetValue<string>("Model");
            if (string.IsNullOrWhiteSpace(systemPrompt) || string.IsNullOrWhiteSpace(model))
            {
                logger.LogWarning("Skipping incomplete LLM config for speciality '{Speciality}'", speciality);
                continue;
            }

            _llmConfigs[speciality] = (systemPrompt, model);
        }
    }

    public static string GetSystemPrompt(LlmSpeciality speciality)
    {
        return _llmConfigs.TryGetValue(speciality, out var config)
            ? config.system
            : throw new KeyNotFoundException("No system prompt registered for speciality " + speciality);
    }
    public static string GetSystemModel(LlmSpeciality speciality)
    {
        return _llmConfigs.TryGetValue(speciality, out var config)
            ? config.model
            : throw new KeyNotFoundException("No model registered for speciality " + speciality);
    }
}