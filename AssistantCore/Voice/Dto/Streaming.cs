using System.Text.Json.Serialization;

namespace AssistantCore.Voice.Dto;

public class SatelliteTtsStart : SatelliteDto
{
    [JsonPropertyName("session_id")] public string SessionId { get; set; }
    [JsonPropertyName("audio_format")] public SatelliteAudioFormat AudioFormat { get; set; }
    [JsonPropertyName("streaming")] public bool Streaming { get; set; }
}

public class SatelliteTtsEnd : SatelliteDto
{
    [JsonPropertyName("session_id")] public string SessionId { get; set; }
}