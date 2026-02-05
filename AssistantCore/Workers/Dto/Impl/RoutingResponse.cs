using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record RoutingResponse : WorkerResponse<RoutingOutput>
{
    [JsonPropertyName("request_id")] public string RequestId { get; init; }
    [JsonPropertyName("usage")] public WorkerUsage Usage { get; init; }
    [JsonPropertyName("output")] public RoutingOutput Output { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }

    public RoutingResponse(string requestId, WorkerUsage usage, RoutingOutput output, string? error)
        : base(requestId, usage, output, error)
    {
        RequestId = requestId;
        Usage = usage;
        Output = output;
        Error = error;
    }
}

public record RoutingOutput
{
    [JsonPropertyName("speciality")] public string Speciality { get; init; }
    [JsonPropertyName("confidence")] public float Confidence { get; init; }
    [JsonPropertyName("reason")] public string Reason { get; init; }

    public RoutingOutput(string speciality, float confidence, string reason)
    {
        Speciality = speciality;
        Confidence = confidence;
        Reason = reason;
    }
}
