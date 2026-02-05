using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record TtsRequest : WorkerRequest<TtsInput, TtsConfig, TtsContext>
{
    [JsonPropertyName("request_id")] public new string RequestId { get; init; }
    [JsonPropertyName("input")] public new TtsInput Input { get; init; }
    [JsonPropertyName("config")] public new TtsConfig Config { get; init; }
    [JsonPropertyName("context")] public new TtsContext Context { get; init; }

    public TtsRequest(string requestId, TtsInput input, TtsConfig config, TtsContext context)
        : base(requestId, input, config, context)
    {
        RequestId = requestId;
        Input = input;
        Config = config;
        Context = context;
    }
}

public record TtsInput
{
    [JsonPropertyName("text")] public string Text { get; init; }

    public TtsInput(string text)
    {
        Text = text;
    }
}

public record TtsConfig
{
    [JsonPropertyName("voice")] public string Voice { get; init; }
    [JsonPropertyName("speed")] public float Speed { get; init; }

    public TtsConfig(string voice, float speed)
    {
        Voice = voice;
        Speed = speed;
    }
}

public record TtsContext();