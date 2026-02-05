using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AssistantCore.Voice.Dto;
using Microsoft.Extensions.Logging;

namespace AssistantCore.Voice;

/// <summary>
/// Represents a WebSocket connection to a satellite.
/// This class manages the WebSocket, the session state machine and validation of messages.
/// The actual processing of audio and text is handled elsewhere, use the OnSessionCompleted event to get a finished user request.
/// </summary>
public class SatelliteConnection
{
    public string ConnectionId { get; }
    private readonly WebSocket _socket;
    private readonly ILogger? _logger;

    public SatelliteConnectionState State { get; private set; }
    public SatelliteHello? SatelliteInfo { get; private set; }

    /// <summary>
    /// Invoked on audio end message. The audio was received and is ready for processing.
    /// </summary>
    public Func<SatelliteSession, Task>? OnSessionCompleted;

    private SatelliteSession? _activeSession;

    private SatelliteConnection(string connectionId, WebSocket socket, ILogger? logger = null)
    {
        ConnectionId = connectionId;
        _socket = socket;
        State = SatelliteConnectionState.Connected;
        _logger = logger;
        _logger?.LogInformation("SatelliteConnection {ConnectionId} created", connectionId);
    }
    public static SatelliteConnection Create(string connectionId, WebSocket socket, ILogger? logger = null)
    {
        return new SatelliteConnection(connectionId, socket, logger);
    }

    public async Task RunAsync(CancellationToken token)
    {
        try
        {
            while (_socket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var buffer = new byte[4096];
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Close:
                        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", token);
                        break;
                    case WebSocketMessageType.Text:
                        await HandleTextMessageAsync(buffer, token);
                        break;
                    case WebSocketMessageType.Binary:
                        HandleByteMessageAsync(buffer, result.Count);
                        break;
                    default:
                        _logger?.LogWarning("Unsupported WebSocket message type for {ConnectionId}", ConnectionId);
                        throw new ArgumentException("Unsupported WebSocket message type");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("Connection canceled for {ConnectionId}", ConnectionId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "RunAsync error for {ConnectionId}", ConnectionId);
        }
    }

    private void HandleByteMessageAsync(byte[] buf, int count)
    {
        // Update state if this is the first audio frame
        if (State == SatelliteConnectionState.SessionActive) State = SatelliteConnectionState.ReceivingAudio;
        if (State != SatelliteConnectionState.ReceivingAudio) return;

        _activeSession!.AppendAudio(buf, count);
        _logger?.LogDebug("Appended {Bytes} audio bytes to session {SessionId} on connection {ConnectionId}", count, _activeSession?.SessionId, ConnectionId);
    }

    private async Task HandleTextMessageAsync(byte[] buf, CancellationToken token)
    {
        var jsonStr = Encoding.UTF8.GetString(buf).TrimEnd('\0');
        _logger?.LogDebug("Received text message on {ConnectionId}: {Payload}", ConnectionId, jsonStr);
        JsonDocument json;
        try
        {
            json = JsonDocument.Parse(jsonStr);
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Failed to parse JSON message on {ConnectionId}", ConnectionId);
            return;
        }

        if (!json.RootElement.TryGetProperty("type", out var messageType))
        {
            _logger?.LogWarning("Message without type field received on {ConnectionId}", ConnectionId);
            return;
        }

        var deserializeOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        try
        {
            switch (messageType.GetString())
            {
                case "hello":
                    if (State != SatelliteConnectionState.Connected) return;

                    SatelliteInfo = JsonSerializer.Deserialize<SatelliteHello>(jsonStr, deserializeOpts);
                    if (SatelliteInfo == null) return;
                    State = SatelliteConnectionState.Ready;
                    _logger?.LogInformation("Satellite {ConnectionId} said hello. Protocol {Protocol}", ConnectionId, SatelliteInfo.ProtocolVersion);
                    var helloAck = new SatelliteHelloAck
                    {
                        Type = "hello.ack",
                        ProtocolVersion = SatelliteInfo.ProtocolVersion,
                        Accepted = true
                    };
                    await SendMessageAsync(helloAck, token);
                    break;
                case "session.start":
                    if (State != SatelliteConnectionState.Ready) return;

                    var sessionStart = JsonSerializer.Deserialize<SatelliteSessionStart>(jsonStr, deserializeOpts);
                    if (sessionStart == null) return;
                    _activeSession = new SatelliteSession(sessionStart);
                    State = SatelliteConnectionState.SessionActive;
                    _logger?.LogInformation("Session {SessionId} started on connection {ConnectionId}", _activeSession.SessionId, ConnectionId);
                    var sessionAck = new SatelliteSessionAck
                    {
                        Type = "session.ack",
                        SessionId = _activeSession.SessionId
                    };
                    await SendMessageAsync(sessionAck, token);
                    break;
                case "audio.end":
                    if (State == SatelliteConnectionState.SessionActive)
                    {
                        _logger?.LogWarning("Audio end received before any audio frames on connection {ConnectionId}", ConnectionId);
                        State = SatelliteConnectionState.ReceivingAudio;
                    }
                    if (State != SatelliteConnectionState.ReceivingAudio) return;

                    var audioEnd = JsonSerializer.Deserialize<SatelliteAudioEnd>(jsonStr, deserializeOpts);
                    if (audioEnd == null) return;
                    State = SatelliteConnectionState.WaitingForProcessing;
                    _logger?.LogInformation("Audio end for session {SessionId} on connection {ConnectionId}", _activeSession?.SessionId, ConnectionId);
                    if (_activeSession != null && OnSessionCompleted != null) await OnSessionCompleted.Invoke(_activeSession);
                    break;
                case "session.abort":
                    if ((int)State < (int)SatelliteConnectionState.SessionActive) return;

                    var sessionAbort = JsonSerializer.Deserialize<SatelliteSessionAbort>(jsonStr, deserializeOpts);
                    if (sessionAbort == null) return;
                    _activeSession = null;
                    State = SatelliteConnectionState.Ready;
                    _logger?.LogInformation("Session aborted on connection {ConnectionId}", ConnectionId);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling text message on {ConnectionId}", ConnectionId);
        }
    }
    
    public async Task SendTtsAsync(byte[] ttsData, CancellationToken token)
    {
        if (State != SatelliteConnectionState.WaitingForProcessing) return;
        State = SatelliteConnectionState.Playback;

        var ttsStart = new SatelliteTtsStart
        {
            Type = "tts.start",
            SessionId = _activeSession!.SessionId,
            AudioFormat = SatelliteInfo!.AudioFormat,
            Streaming = true
        };
        await SendMessageAsync(ttsStart, token);

        int frameSize = GetAudioFrameSize();
        if (frameSize <= 0)
        {
            _logger?.LogWarning("Invalid audio frame size for connection {ConnectionId}", ConnectionId);
            await SendErrorAsync("invalid_audio_format", "Unsupported audio format: " + SatelliteInfo.AudioFormat.Encoding,
                token);
            return;
        }
        for (int offset = 0; offset < ttsData.Length; offset += frameSize)
        {
            int chunkSize = Math.Min(frameSize, ttsData.Length - offset);
            await _socket.SendAsync(new ArraySegment<byte>(ttsData, offset, chunkSize), 
                WebSocketMessageType.Binary, true, token);
        }

        var ttsEnd = new SatelliteTtsEnd
        {
            Type = "tts.end",
            SessionId = _activeSession!.SessionId
        };
        await SendMessageAsync(ttsEnd, token);
        State = SatelliteConnectionState.Connected;
        _logger?.LogInformation("Finished TTS playback for session {SessionId} on connection {ConnectionId}", _activeSession.SessionId, ConnectionId);
    }

    public async Task SendErrorAsync(string code, string message, CancellationToken token)
    {
        var error = new SatelliteError
        {
            Type = "error",
            SessionId = _activeSession?.SessionId ?? string.Empty,
            ErrorCode = code,
            Message = message
        };
        _logger?.LogWarning("Sending error to connection {ConnectionId}: {Code} {Message}", ConnectionId, code, message);
        await SendMessageAsync(error, token);

        State = SatelliteConnectionState.Connected;
    }

    public async Task SendBargeInAsync(CancellationToken token)
    {
        if (State != SatelliteConnectionState.Playback) return; // Can only barge in during tts playback
        _logger?.LogInformation("Barge-in received for connection {ConnectionId}", ConnectionId);
        var bargeIn = new SatelliteBargeIn
        {
            Type = "barge_in",
            SessionId = _activeSession!.SessionId
        };
        await SendMessageAsync(bargeIn, token);
    }

    public async Task SendMessageAsync(SatelliteDto message, CancellationToken token)
    {
        var json = JsonSerializer.Serialize(message, message.GetType()); // Use GetType to preserve runtime type
        var buffer = Encoding.UTF8.GetBytes(json);
        _logger?.LogDebug("Sending message to {ConnectionId}: {Message}", ConnectionId, json);
        await _socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, token);
    }

    private int GetAudioFrameSize()
    {
        if (SatelliteInfo == null) return 0;
        if (SatelliteInfo.AudioFormat.Encoding != "pcm_s16le")
            throw new NotSupportedException("Only pcm_s16le encoding is supported");
        
        // TODO: support more audio formats, parse dynamically
        int bytesPerSample = 2; // 16 bits
        int channels = SatelliteInfo.AudioFormat.Channels;
        int sampleRate = SatelliteInfo.AudioFormat.SampleRate;
        int frameDurationMs = SatelliteInfo.AudioFormat.FrameMs;

        return (sampleRate * bytesPerSample * channels * frameDurationMs) / 1000;
    }
}

