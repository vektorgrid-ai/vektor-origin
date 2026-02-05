using AssistantCore.Workers.Dto.Impl;

namespace AssistantCore.Workers;

public interface ISttWorkerClient
{
    Task<SttResponse> InferAsync(
        WorkerDescriptor worker,
        SttRequest input,
        CancellationToken ct);
}