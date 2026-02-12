using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;

public class RecentLogSink(int capacity = 1000) : ILogEventSink, IDisposable
{
    private readonly ConcurrentQueue<LogEvent> _queue = new();
    private readonly Lock _trimLock = new();

    public void Emit(LogEvent logEvent)
    {
        _queue.Enqueue(logEvent);
        // trim if necessary
        if (_queue.Count > capacity)
        {
            lock (_trimLock)
            {
                while (_queue.Count > capacity && _queue.TryDequeue(out _)) { }
            }
        }
    }

    // Return a snapshot (format or map to DTO)
    public LogEvent[] GetSnapshot() => _queue.ToArray();

    public void Dispose() { /* nothing for now */ }
}