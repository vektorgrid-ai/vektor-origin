using System.Collections.Concurrent;
using AssistantCore.Agents;
using AssistantCore.Workers;
using AssistantCore.Chat;
using AssistantCore.Tools;
using AssistantCore.Workers.LoadBalancing;
using Microsoft.Extensions.Logging;

namespace AssistantCore.Voice;

public class SatelliteManager
{
    private ISttWorkerClient _stt;
    private IRoutingWorkerClient _router;
    private ITtsWorkerClient _tts;
    private WorkerRegistry _registry;
    private ILoadBalancer _balancer;
    private ChatManager _chat;
    private LlmAgent _agent;
    private readonly ILogger<SatelliteManager> _logger;

    private readonly ConcurrentDictionary<string, (SatelliteConnection connection, CancellationTokenSource cts)> _activePipelines = new();

    public SatelliteManager(
        WorkerRegistry registry,
        ILoadBalancer balancer,
        ISttWorkerClient stt,
        IRoutingWorkerClient router,
        ITtsWorkerClient tts,
        ChatManager chat,
        LlmAgent agent,
        ILogger<SatelliteManager> logger)
    {
        _stt = stt;
        _router = router;
        _tts = tts;
        
        _registry = registry;
        _balancer = balancer;
        _chat = chat;
        _agent = agent;
        _logger = logger;

    }

    public void RegisterConnection(SatelliteConnection connection)
    {
        _logger.LogInformation("Registering connection {ConnectionId}", connection.ConnectionId);
        connection.OnSessionCompleted += session => HandleSessionCompletedAsync(connection, session);
    }

    public void UnregisterConnection(SatelliteConnection connection)
    {
        _logger.LogInformation("Unregistering connection {ConnectionId}", connection.ConnectionId);
        CancelPipeline(connection.ConnectionId);
    }

    public SatelliteConnection[] GetActiveConnections() => _activePipelines.Values
        .Select(v => v.connection)
        .ToArray();

    private async Task HandleSessionCompletedAsync(SatelliteConnection connection, SatelliteSession session)
    {
        _logger.LogInformation("Handling completed session {SessionId} for connection {ConnectionId}", session.SessionId, connection.ConnectionId);
        CancelPipeline(connection.ConnectionId); // cancel any existing pipeline for this connection

        var cts = new CancellationTokenSource();
        _activePipelines[connection.ConnectionId] = (connection, cts);
        
        var orchestrator = new VoiceSessionOrchestrator(
            session, 
            connection, 
            _stt, _router, _tts,
            _chat, _agent,
            _registry, 
            _balancer,
            _logger);

        try
        {
            await orchestrator.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Pipeline cancelled for connection {ConnectionId}", connection.ConnectionId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while processing session {SessionId} for connection {ConnectionId}", session.SessionId, connection.ConnectionId);
        }
        finally
        {
            _activePipelines.TryRemove(connection.ConnectionId, out _);
            _logger.LogInformation("Pipeline finished for connection {ConnectionId}", connection.ConnectionId);
        }
    }

    public void CancelPipeline(string connectionId)
    {
        if (_activePipelines.TryRemove(connectionId, out var connectionInfo))
        {
            _logger.LogInformation("Cancelling pipeline for connection {ConnectionId}", connectionId);
            connectionInfo.cts.Cancel();
        }
    }

    public void CancelAllPipelines()
    {
        _logger.LogInformation("Cancelling all active pipelines ({Count})", _activePipelines.Count);
        foreach (var kv in _activePipelines)
        {
            if (_activePipelines.TryRemove(kv.Key, out var connectionInfo))
            {
                try
                {
                    connectionInfo.cts.Cancel();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error cancelling pipeline {ConnectionId}", kv.Key);
                }
            }
        }
    }
}