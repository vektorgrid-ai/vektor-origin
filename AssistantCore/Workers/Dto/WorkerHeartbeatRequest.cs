using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto;

public struct WorkerHeartbeatRequest
{
    [JsonPropertyName("worker_id")] public string WorkerId { get; set; }
}