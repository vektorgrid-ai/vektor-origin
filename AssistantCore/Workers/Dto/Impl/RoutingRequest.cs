using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record RoutingRequest : WorkerRequest<RoutingInput, RoutingConfig, RoutingContext>
{
    public RoutingRequest(string requestId, RoutingInput input, RoutingConfig config, RoutingContext context)
        : base(requestId, input, config, context)
    {
    }
}

public record RoutingInput
{
    [JsonPropertyName("text")] public string Text { get; init; }

    public RoutingInput(string text)
    {
        Text = text;
    }
}

public record RoutingConfig
{
    [JsonPropertyName("allowed_specialities")] public string[] AllowedSpecialities { get; init; }

    public RoutingConfig(string[] allowedSpecialities)
    {
        AllowedSpecialities = allowedSpecialities;
    }
}

public record RoutingContext
{
    [JsonPropertyName("location")] public string Location { get; init; }

    public RoutingContext(string location)
    {
        Location = location;
    }
}
