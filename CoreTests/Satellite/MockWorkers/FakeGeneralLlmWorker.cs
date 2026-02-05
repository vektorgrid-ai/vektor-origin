using AssistantCore.Workers;
using AssistantCore.Chat;
using AssistantCore.Workers.Dto;
using AssistantCore.Workers.Dto.Impl;

namespace CoreTests.Satellite.MockWorkers;

public class FakeGeneralLlmWorker : ILlmWorkerClient
{
    public LlmSpeciality Speciality => LlmSpeciality.General;

    public Task<LlmResponse> InferAsync(WorkerDescriptor worker, LlmRequest input, CancellationToken ct)
    {
        return Task.FromResult(new LlmResponse("0", new WorkerUsage("fake-llm", 1),
            new LlmOutput("This is a fake LLM response."), null));
    }
}