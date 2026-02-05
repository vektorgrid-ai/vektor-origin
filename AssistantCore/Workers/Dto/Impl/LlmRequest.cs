using AssistantCore.Chat;
using AssistantCore.Tools.Dto;
using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record LlmRequest : WorkerRequest<LlmInput, LlmConfig, LlmContext>
{
    public LlmRequest(string requestId, LlmInput input, LlmConfig config, LlmContext context)
        : base(requestId, input, config, context)
    {
    }
}

public record LlmInput
{
    [JsonPropertyName("prompt")] public string Prompt { get; init; }
    [JsonPropertyName("tools")] public ToolDto[] Tools { get; init; }
    [JsonPropertyName("chat_context")] public ChatContext ChatContext { get; init; }

    public LlmInput(string prompt, ToolDto[] tools, ChatContext chatContext)
    {
        Prompt = prompt;
        Tools = tools;
        ChatContext = chatContext;
    }
}

public record LlmConfig
{
    [JsonPropertyName("max_tokens")] public int MaxTokens { get; init; }
    [JsonPropertyName("temperature")] public float Temperature { get; init; }

    public LlmConfig(int maxTokens, float temperature)
    {
        MaxTokens = maxTokens;
        Temperature = temperature;
    }
}

public record LlmContext
{
    [JsonPropertyName("location")] public string Location { get; init; }

    public LlmContext(string location)
    {
        Location = location;
    }
}
