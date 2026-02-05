namespace AssistantCore.Workers;

public sealed class WorkerDescriptor
{
    public string WorkerId { get; init; } = default!;
    public WorkerType Type { get; init; }
    public Uri Endpoint { get; init; } = default!;
    public WorkerCapabilities Capabilities { get; init; } = new();
    public DateTime LastSeenUtc { get; set; }
}