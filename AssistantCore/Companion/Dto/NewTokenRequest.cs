using System.Text.Json.Serialization;

namespace AssistantCore.Companion.Dto;

public struct NewTokenRequest
{
    [JsonPropertyName("device_id")] public string DeviceId { get; set; }
    [JsonPropertyName("device_name")] public string DeviceName { get; set; }
    [JsonPropertyName("token")] public string Token { get; set; }
}