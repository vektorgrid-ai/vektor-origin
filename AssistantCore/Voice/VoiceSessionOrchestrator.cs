using AssistantCore.Agents;
using AssistantCore.Chat;
using AssistantCore.Tools;
using AssistantCore.Workers;
using AssistantCore.Workers.Dto.Impl;
using AssistantCore.Workers.LoadBalancing;

namespace AssistantCore.Voice;

/// <summary>
/// Handles the processing of a voice session from audio received to response sent.
/// It exists after the audio end message is received until the response is sent back to the satellite.
/// </summary>
public class VoiceSessionOrchestrator(
    SatelliteSession session,
    SatelliteConnection connection,
    ISttWorkerClient stt,
    IRoutingWorkerClient router,
    ITtsWorkerClient tts,
    ChatManager chat,
    LlmAgent agent,
    WorkerRegistry registry,
    ILoadBalancer balancer,
    ILogger<SatelliteManager> parentLogger)
{
    private readonly ILogger _logger = parentLogger;

    public async Task RunAsync(CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Orchestrator starting for session {SessionId} on connection {ConnectionId}", session.SessionId, connection.ConnectionId);

            var text = await InferSttAsync(session.AudioBytes, token);
            _logger.LogInformation("Transcription for session {SessionId}: {Text}", session.SessionId, text);

            var speciality = await InferRouterAsync(text, token);
            _logger.LogInformation("Routed session {SessionId} to speciality {Speciality}", session.SessionId, speciality);

            _logger.LogInformation(" handing off to Agent for session {SessionId}", session.SessionId);
            var response = await agent.ProcessAsync(text, chat, speciality, connection.SatelliteInfo?.Area ?? "unknown", token);
            _logger.LogInformation("LLM response preview for session {SessionId}: {ResponsePreview}...", session.SessionId, response[..Math.Min(200, response.Length)]);

            var audioBytes = await InferTtsAsync(response, token);
            _logger.LogInformation("Synthesized audio for session {SessionId}, {ByteCount} bytes", session.SessionId, audioBytes.Length);

            await connection.SendTtsAsync(audioBytes, token, response);
            _logger.LogInformation("Sent TTS for session {SessionId}", session.SessionId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Orchestrator cancelled for session {SessionId}", session.SessionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in orchestrator for session {SessionId} on connection {ConnectionId}", session.SessionId, connection.ConnectionId);
            throw;
        }
    }

    private async Task<string> InferSttAsync(byte[] audioBytes, CancellationToken token)
    {
        var candidates = registry.GetAliveWorkersOfType(WorkerType.Stt);
        var worker = balancer.Select(candidates, "stt");
        var input = new SttRequest("0", new SttInput(audioBytes, "pcm_s16le", 16000, 1),
            new SttConfig(), new SttContext(connection.SatelliteInfo?.Area ?? "unknown")); // TODO: fill in values dynamically
        var result = await stt.InferAsync(worker, input, token);
        return result.Output.Text;
    }
    private async Task<LlmSpeciality> InferRouterAsync(string text, CancellationToken token)
    {
        var candidates = registry.GetAliveWorkersOfType(WorkerType.Router);
        var worker = balancer.Select(candidates, "router");
        var specialities = Enum.GetNames<LlmSpeciality>()
            .Where(n => n != nameof(LlmSpeciality.None) && n != nameof(LlmSpeciality.All))
            .ToArray();
        var input = new RoutingRequest("0", new RoutingInput(text), new RoutingConfig(specialities),
            new RoutingContext(connection.SatelliteInfo?.Area ?? "unknown")); // TODO: fill in values dynamically
        var result = await router.InferAsync(worker, input, token);

        var specialityStr = result.Output.Speciality;
        if (!Enum.TryParse<LlmSpeciality>(specialityStr, out var speciality))
            speciality = LlmSpeciality.General;
        
        return speciality;
    }
    private async Task<byte[]> InferTtsAsync(string text, CancellationToken token)
    {
        var candidates = registry.GetAliveWorkersOfType(WorkerType.Tts);
        var worker = balancer.Select(candidates, "tts");
        // TODO: fill in values dynamically
        var input = new TtsRequest("0", new TtsInput(text), new TtsConfig(null!, 1f), new TtsContext());
        var result = await tts.InferAsync(worker, input, token);
        return result.Output.AudioData;
    }
}