using AssistantCore.Workers.Dto.Impl;

namespace AssistantCore.Workers;

public interface IRoutingWorkerClient
{
    Task<RoutingResponse> InferAsync(
        WorkerDescriptor worker,
        RoutingRequest input,
        CancellationToken ct);
}