namespace AssistantCore.Workers.Dto;

using System.Text.Json.Serialization;

public record WorkerRequest<TInput, TConfig, TContext>
{
    [JsonPropertyName("request_id")] public string RequestId { get; init; }
    [JsonPropertyName("input")] public TInput Input { get; init; }
    [JsonPropertyName("config")] public TConfig Config { get; init; }
    [JsonPropertyName("context")] public TContext Context { get; init; }

    public WorkerRequest(string requestId, TInput input, TConfig config, TContext context)
    {
        RequestId = requestId;
        Input = input;
        Config = config;
        Context = context;
    }
}
