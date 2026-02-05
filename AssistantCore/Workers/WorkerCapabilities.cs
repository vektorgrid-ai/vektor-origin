namespace AssistantCore.Workers;

public sealed class WorkerCapabilities
{
    // Optional feature flags
    public bool SupportsStreaming { get; init; }
    public bool SupportsTools { get; init; }

    // Optional metadata
    public IReadOnlyList<string>? Models { get; init; }
}