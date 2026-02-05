using AssistantCore.Workers;
using AssistantCore.Workers.Dto;
using AssistantCore.Workers.Dto.Impl;

namespace CoreTests.Satellite.MockWorkers;

public class FakeSttWorker : ISttWorkerClient
{
    public Task<SttResponse> InferAsync(WorkerDescriptor worker, SttRequest input, CancellationToken ct)
    {
        return Task.FromResult(new SttResponse("0", new SttOutput("Hello world", 1, "en"),
            new WorkerUsage("fake-stt", 1), null));
    }
}