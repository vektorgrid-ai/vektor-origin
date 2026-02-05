using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto;

public struct WorkerRegisterResult
{
    [JsonPropertyName("accepted")] public bool Accepted { get; set; }
    [JsonPropertyName("worker_id")] public string WorkerId { get; set; }
}