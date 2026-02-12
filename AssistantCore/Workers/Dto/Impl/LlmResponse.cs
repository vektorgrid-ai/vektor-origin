using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record LlmResponse : WorkerResponse<LlmOutput>
{
    public LlmResponse(string requestId, WorkerUsage usage, LlmOutput output, string? error)
        : base(requestId, usage, output, error)
    {
    }
}

public record LlmOutput
{
    [JsonPropertyName("text")] public string Text { get; init; }
    [JsonPropertyName("tool_calls")] public List<LlmToolCall> ToolCalls { get; set; }

    public LlmOutput(string text)
    {
        Text = text;
    }
}

public record LlmToolCall
{
    [JsonPropertyName("name")] public string ToolName { get; set; }
    [JsonPropertyName("arguments")] public string JsonArgs { get; set; }

    public LlmToolCall(string toolName, string jsonArgs)
    {
        ToolName = toolName;
        JsonArgs = jsonArgs;
    }
}
