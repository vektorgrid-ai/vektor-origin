using System.Text.Json.Serialization;

namespace AssistantCore.Voice.Dto;

public class SatelliteHello : SatelliteDto
{
    [JsonPropertyName("protocol_version")] public int ProtocolVersion { get; set; }
    [JsonPropertyName("satellite_id")] public string SatelliteId { get; set; }
    [JsonPropertyName("area")] public string Area { get; set; }
    [JsonPropertyName("language")] public string Language { get; set; }
    [JsonPropertyName("capabilities")] public SatelliteCapabilities Capabilities { get; set; }
    [JsonPropertyName("audio_format")] public SatelliteAudioFormat AudioFormat { get; set; }
}

public class SatelliteHelloAck : SatelliteDto
{
    [JsonPropertyName("protocol_version")] public int ProtocolVersion { get; set; }
    [JsonPropertyName("accepted")] public bool Accepted { get; set; }
}

public struct SatelliteCapabilities
{
    [JsonPropertyName("speaker")] public bool Speaker { get; set; }
    [JsonPropertyName("display")] public bool Display { get; set; }
    [JsonPropertyName("supports_barge_in")] public bool SupportsBargeIn { get; set; }
    [JsonPropertyName("supports_streaming_tts")] public bool SupportsStreamingTts { get; set; }
}

public struct SatelliteAudioFormat
{
    [JsonPropertyName("encoding")] public string Encoding { get; set; }
    [JsonPropertyName("sample_rate")] public int SampleRate { get; set; }
    [JsonPropertyName("channels")] public int Channels { get; set; }
    [JsonPropertyName("frame_ms")] public int FrameMs { get; set; }
}