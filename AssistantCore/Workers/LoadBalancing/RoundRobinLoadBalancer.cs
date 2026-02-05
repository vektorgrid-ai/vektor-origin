using System.Collections.Concurrent;

namespace AssistantCore.Workers.LoadBalancing;

public sealed class RoundRobinLoadBalancer : ILoadBalancer
{
    private readonly ConcurrentDictionary<string, int> _counters = new();

    public WorkerDescriptor Select(
        IReadOnlyList<WorkerDescriptor> workers,
        string key)
    {
        if (workers.Count == 0)
            throw new InvalidOperationException("No available workers");

        var index = _counters.AddOrUpdate(
            key,
            0,
            (_, current) => (current + 1) % workers.Count);

        return workers[index];
    }
}