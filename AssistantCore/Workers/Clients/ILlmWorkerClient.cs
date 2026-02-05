using AssistantCore.Chat;
using AssistantCore.Workers.Dto.Impl;

namespace AssistantCore.Workers;

public interface ILlmWorkerClient
{
    Task<LlmResponse> InferAsync(
        WorkerDescriptor worker,
        LlmRequest input,
        CancellationToken ct);
}