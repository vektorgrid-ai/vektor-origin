namespace AssistantCore.Workers.Dto;

using System.Text.Json.Serialization;

public record WorkerResponse<TOutput>
{
    [JsonPropertyName("request_id")] public string RequestId { get; init; }
    [JsonPropertyName("usage")] public WorkerUsage Usage { get; init; }
    [JsonPropertyName("output")] public TOutput Output { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }

    public WorkerResponse(string requestId, WorkerUsage usage, TOutput output, string? error)
    {
        RequestId = requestId;
        Usage = usage;
        Output = output;
        Error = error;
    }
}

public record WorkerUsage(string Model, int LatencyMs);