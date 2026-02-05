using AssistantCore.Workers.Dto.Impl;

namespace AssistantCore.Workers;

public class HttpRoutingWorkerClient(HttpClient http) : IRoutingWorkerClient
{
    public async Task<RoutingResponse> InferAsync(WorkerDescriptor worker, RoutingRequest input, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync(
            new Uri(worker.Endpoint, "/infer"),
            input,
            ct);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoutingResponse>(ct)
               ?? throw new InvalidOperationException("Failed to deserialize RoutingResponse");
    }
}