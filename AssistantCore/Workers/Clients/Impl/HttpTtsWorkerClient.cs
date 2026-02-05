using AssistantCore.Workers.Dto.Impl;

namespace AssistantCore.Workers;

public class HttpTtsWorkerClient(HttpClient http) : ITtsWorkerClient
{
    public async Task<TtsResponse> InferAsync(WorkerDescriptor worker, TtsRequest input, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync(
            new Uri(worker.Endpoint, "/infer"),
            input,
            ct);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TtsResponse>(ct)
               ?? throw new InvalidOperationException("Failed to deserialize TtsResponse");
    }
}