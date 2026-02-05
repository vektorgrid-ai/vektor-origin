using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto;

public struct WorkerRegisterRequest
{
    [JsonPropertyName("type")] public string WorkerType { get; set; }
    [JsonPropertyName("endpoint")] public string Endpoint { get; set; }
    [JsonPropertyName("speciality")] public string? Speciality { get; set; } // Required for LLM workers, optional otherwise
    // TODO: Capabilities
}