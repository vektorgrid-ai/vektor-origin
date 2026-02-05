using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;

namespace AssistantCore.Logging;

public class FileLogger : ILogger
{
    private readonly string _name;
    private readonly FileLoggerProvider _provider;

    public FileLogger(string name, FileLoggerProvider provider)
    {
        _name = name;
        _provider = provider;
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var message = formatter(state, exception);
        var sb = new StringBuilder();
        sb.Append(DateTime.UtcNow.ToString("o"));
        sb.Append(' ');
        sb.Append(logLevel.ToString());
        sb.Append(" [");
        sb.Append(_name);
        sb.Append("] ");
        sb.Append(message);
        if (exception != null)
        {
            sb.Append('\n');
            sb.Append(exception.ToString());
        }

        _provider.WriteLine(sb.ToString());
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly BlockingCollection<string> _queue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _outputTask;

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        _outputTask = Task.Run(ProcessQueueAsync);
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, this);

    public void Dispose()
    {
        _queue.CompleteAdding();
        _cts.Cancel();
        try { _outputTask.Wait(1000); } catch { }
    }

    internal void WriteLine(string line)
    {
        // best-effort enqueue
        try { _queue.Add(line); } catch { }
    }

    private async Task ProcessQueueAsync()
    {
        var fileMode = FileMode.Append;
        try
        {
            await using var fs = new FileStream(_filePath, fileMode, FileAccess.Write, FileShare.Read);
            await using var sw = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };
            foreach (var line in _queue.GetConsumingEnumerable(_cts.Token))
            {
                await sw.WriteLineAsync(line);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            // swallow - logging should not crash the app
        }
    }
}

