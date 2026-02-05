using AssistantCore.Chat;
using AssistantCore.Workers.Dto.Impl;

namespace AssistantCore.Workers;

public sealed class HttpLlmWorkerClient(HttpClient http) : ILlmWorkerClient
{
    public async Task<LlmResponse> InferAsync(
        WorkerDescriptor worker,
        LlmRequest input,
        CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync(
            new Uri(worker.Endpoint, "/infer"),
            input,
            ct);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LlmResponse>(ct)
               ?? throw new InvalidOperationException("Failed to deserialize LlmResponse");
    }
}