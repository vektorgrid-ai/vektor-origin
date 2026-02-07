using System.Text.Json.Serialization;

namespace AssistantCore.Companion.Dto;

public struct ToolAnswer
{
    [JsonPropertyName("request_id")] public string RequestId { get; set; }
    [JsonPropertyName("device_id")] public string DeviceId { get; set; }
    [JsonPropertyName("decision")] public string Decision { get; set; }
    [JsonPropertyName("timestamp")] public string Timestamp { get; set; }
    [JsonPropertyName("signature")] public string Signature { get; set; }
    [JsonPropertyName("public_key")] public string PublicKey { get; set; }
}