using System.Collections.Concurrent;

namespace AssistantCore.Workers;

public sealed class WorkerRegistry(ILogger<WorkerRegistry> logger)
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(60);
    private readonly ConcurrentDictionary<string, WorkerDescriptor> _workers = new();

    public void Register(WorkerDescriptor worker)
    {
        worker.LastSeenUtc = DateTime.UtcNow;
        _workers[worker.WorkerId] = worker;
        logger.LogInformation("Registered worker {WorkerId} of type {WorkerType} at {Endpoint}",
            worker.WorkerId, worker.Type, worker.Endpoint);
    }

    public void Heartbeat(string workerId)
    {
        if (_workers.TryGetValue(workerId, out var worker))
        {
            worker.LastSeenUtc = DateTime.UtcNow;
            logger.LogDebug("Received heartbeat from worker {WorkerId}, timeout at {Timeout}", workerId,
                worker.LastSeenUtc + _timeout);
        }
    }

    public IReadOnlyList<WorkerDescriptor> GetAliveWorkersOfType(WorkerType type)
    {
        logger.LogDebug("Retrieving workers for type {WorkerType}", type);

        return (from w in GetAliveWorkers() 
                where w.Type == type
                select w)
                .ToList();
    }
    
    public WorkerDescriptor? GetWorker(string workerId) =>
        _workers.GetValueOrDefault(workerId);
    
    public WorkerDescriptor[] GetAllWorkers() =>
        _workers.Values.ToArray();
    
    public WorkerDescriptor[] GetAliveWorkers()
    {
        var now = DateTime.UtcNow;
        var valid = new List<WorkerDescriptor>();
        foreach (var w in _workers.Values)
        {
            bool alive = (now - w.LastSeenUtc) < _timeout;
            if (!alive)
                logger.LogInformation("Worker {WorkerId} of type {WorkerType} at {Endpoint} is considered dead (last seen at {LastSeen} UTC)",
                    w.WorkerId, w.Type, w.Endpoint, w.LastSeenUtc);
            else 
                valid.Add(w);
        }
        return valid.ToArray();
    }
}