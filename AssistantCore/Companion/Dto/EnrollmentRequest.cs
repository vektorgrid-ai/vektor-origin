using System.Text.Json.Serialization;

namespace AssistantCore.Companion.Dto;

public struct EnrollmentRequest
{
    [JsonPropertyName("device_name")] public string DeviceName { get; set; }
    [JsonPropertyName("public_key")] public string PublicKey { get; set; }
    [JsonPropertyName("firebase_token")] public string? FirebaseToken { get; set; }
    [JsonPropertyName("device_id")] public string DeviceId { get; set; }
}