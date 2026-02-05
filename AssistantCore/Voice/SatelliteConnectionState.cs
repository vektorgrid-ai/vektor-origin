namespace AssistantCore.Voice;

/// <summary>
/// Represents the state of a satellite connection.
/// </summary>
public enum SatelliteConnectionState
{
    /// <summary>
    /// The WebSocket connection is established but the satellite has not sent its hello message yet.
    /// </summary>
    Connected, 
    /// <summary>
    /// Connection is initialized and capabilities are known. The satellite is ready to start a session.
    /// </summary>
    Ready, 
    /// <summary>
    /// A session was started but no audio is being received yet.
    /// </summary>
    SessionActive, 
    /// <summary>
    /// Currently receiving audio data from the satellite.
    /// </summary>
    ReceivingAudio, 
    /// <summary>
    /// All audio has been received and is being processed.
    /// </summary>
    WaitingForProcessing, 
    /// <summary>
    /// The response is being played back to the satellite.
    /// </summary>
    Playback
}