using AssistantCore.Workers.Dto.Impl;

namespace AssistantCore.Workers;

public class HttpSttWorkerClient(HttpClient http) : ISttWorkerClient
{
    public async Task<SttResponse> InferAsync(WorkerDescriptor worker, SttRequest input, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync(
            new Uri(worker.Endpoint, "/infer"),
            input,
            ct);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SttResponse>(ct)
               ?? throw new InvalidOperationException("Failed to deserialize SttResponse");
    }
}