using AssistantCore.Workers.Dto.Impl;

namespace AssistantCore.Workers;

public interface ITtsWorkerClient
{
    Task<TtsResponse> InferAsync(
        WorkerDescriptor worker,
        TtsRequest input,
        CancellationToken ct);
}