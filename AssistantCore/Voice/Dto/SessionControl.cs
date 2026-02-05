using System.Text.Json.Serialization;

namespace AssistantCore.Voice.Dto;

public class SatelliteSessionStart : SatelliteDto
{
    [JsonPropertyName("session_id")] public string SessionId { get; set; }
    [JsonPropertyName("timestamp")] public long Timestamp { get; set; }
}

public class SatelliteSessionAck : SatelliteDto
{
    [JsonPropertyName("session_id")] public string SessionId { get; set; }
}

public class SatelliteSessionAbort : SatelliteDto
{
    [JsonPropertyName("session_id")] public string SessionId { get; set; }
    [JsonPropertyName("reason")] public string Reason { get; set; }
}

public class SatelliteAudioEnd : SatelliteDto
{
    [JsonPropertyName("session_id")] public string SessionId { get; set; }
    [JsonPropertyName("reason")] public string Reason { get; set; }    
}

public class SatelliteError : SatelliteDto
{
    [JsonPropertyName("session_id")] public string SessionId { get; set; }
    [JsonPropertyName("code")] public string ErrorCode { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; }
}

public class SatelliteBargeIn : SatelliteDto
{
    [JsonPropertyName("session_id")] public string SessionId { get; set; }
}