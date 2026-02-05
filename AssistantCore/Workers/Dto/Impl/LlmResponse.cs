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

    public LlmOutput(string text)
    {
        Text = text;
    }
}
