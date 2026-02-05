using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record SttResponse : WorkerResponse<SttOutput>
{
    public SttResponse(string requestId, SttOutput output, WorkerUsage usage, string? error)
        : base(requestId, usage, output, error)
    {
    }
}

public record SttOutput
{
    [JsonPropertyName("text")] public string Text { get; init; }
    [JsonPropertyName("confidence")] public float Confidence { get; init; }
    [JsonPropertyName("language")] public string Language { get; init; }

    public SttOutput(string text, float confidence, string language)
    {
        Text = text;
        Confidence = confidence;
        Language = language;
    }
}
