using AssistantCore.Workers;
using AssistantCore.Workers.Dto;
using AssistantCore.Workers.Dto.Impl;

namespace CoreTests.Satellite.MockWorkers;

public class FakeRoutingWorker : IRoutingWorkerClient
{
    public Task<RoutingResponse> InferAsync(WorkerDescriptor worker, RoutingRequest input, CancellationToken ct)
    {
        return Task.FromResult(new RoutingResponse("0", new WorkerUsage("fake-router", 1),
            new RoutingOutput("general", 1, ""), null));
    }
}