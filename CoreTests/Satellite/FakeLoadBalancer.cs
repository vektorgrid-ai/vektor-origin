using AssistantCore.Workers;
using AssistantCore.Workers.LoadBalancing;

namespace CoreTests.Satellite;

public class FakeLoadBalancer : ILoadBalancer
{
    public WorkerDescriptor Select(IReadOnlyList<WorkerDescriptor> workers, string key)
    {
        WorkerType type;
        if (key.StartsWith("stt")) type = WorkerType.Stt;
        else if (key.StartsWith("router")) type = WorkerType.Router;
        else if (key.StartsWith("llm")) type = WorkerType.Llm;
        else if (key.StartsWith("tts")) type = WorkerType.Tts;
        else throw new ArgumentException("Unknown worker type key: " + key);
        return new WorkerDescriptor
        {
            WorkerId = "0",
            Capabilities = new WorkerCapabilities(),
            Endpoint = new Uri("https://example.com"),
            LastSeenUtc = DateTime.Now,
            Type = type
        };
    }
}