using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record SttRequest : WorkerRequest<SttInput, SttConfig, SttContext>
{
    public SttRequest(string requestId, SttInput input, SttConfig config, SttContext context)
        : base(requestId, input, config, context)
    {
    }
}

public record SttInput
{
    [JsonPropertyName("data_base64")] public byte[] AudioData { get; init; }
    [JsonPropertyName("encoding")] public string Encoding { get; init; }
    [JsonPropertyName("sample_rate")] public int SampleRate { get; init; }
    [JsonPropertyName("channels")] public int Channels { get; init; }

    public SttInput(byte[] audioData, string encoding, int sampleRate, int channels)
    {
        AudioData = audioData;
        Encoding = encoding;
        SampleRate = sampleRate;
        Channels = channels;
    }
}

public record SttConfig;

public record SttContext
{
    [JsonPropertyName("location")] public string Location { get; init; }

    public SttContext(string location)
    {
        Location = location;
    }
}
