using AssistantCore.Voice.Dto;

namespace AssistantCore.Voice;

/// <summary>
/// Represents a single voice session with a satellite.
/// A session starts with a SatelliteSessionStart message and ends with a SatelliteAudioEnd message and represents the audio data received from the Satellite.
/// </summary>
public class SatelliteSession
{
    public SatelliteSessionStart SessionInfo { get; }
    public string SessionId { get; }
    public byte[] AudioBytes => _audioBuffer.ToArray();

    private readonly MemoryStream _audioBuffer;
    private int _audioByteCount;

    public SatelliteSession(SatelliteSessionStart sessionInfo)
    {
        SessionId = sessionInfo.SessionId;
        SessionInfo = sessionInfo;
        _audioBuffer = new MemoryStream();
    }

    public void AppendAudio(byte[] audioData, int count)
    {
        _audioBuffer.Position = _audioBuffer.Length;
        _audioBuffer.Write(audioData, 0, count);
        _audioByteCount += count;
    }
}