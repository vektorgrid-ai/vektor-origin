namespace AssistantCore.Workers.LoadBalancing;

public interface ILoadBalancer
{
    public WorkerDescriptor Select(
        IReadOnlyList<WorkerDescriptor> workers,
        string key);
}