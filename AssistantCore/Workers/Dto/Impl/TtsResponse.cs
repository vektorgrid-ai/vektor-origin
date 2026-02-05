using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record TtsResponse : WorkerResponse<TtsOutput>
{
    [JsonPropertyName("request_id")] public new string RequestId { get; init; }
    [JsonPropertyName("usage")] public new WorkerUsage Usage { get; init; }
    [JsonPropertyName("output")] public new TtsOutput Output { get; init; }
    [JsonPropertyName("error")] public new string? Error { get; init; }

    public TtsResponse(string requestId, WorkerUsage usage, TtsOutput output, string? error)
        : base(requestId, usage, output, error)
    {
        RequestId = requestId;
        Usage = usage;
        Output = output;
        Error = error;
    }
}

public record TtsOutput
{
    [JsonPropertyName("data_base64")] public byte[] AudioData { get; init; }
    [JsonPropertyName("encoding")] public string Encoding { get; init; }
    [JsonPropertyName("sample_rate")] public int SampleRate { get; init; }
    [JsonPropertyName("channels")] public int Channels { get; init; }

    public TtsOutput(byte[] audioData, string encoding, int sampleRate, int channels)
    {
        AudioData = audioData;
        Encoding = encoding;
        SampleRate = sampleRate;
        Channels = channels;
    }
}
