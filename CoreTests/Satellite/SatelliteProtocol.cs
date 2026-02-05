internal static class SatelliteProtocol
{
    public const int AudioFrameSize = 640;
    public const int TtsFrames = 3;

    public const string Hello = """
                                {
                                  "type": "hello",
                                  "protocol_version": 1,
                                  "mic_id": "kitchen_satellite",
                                  "area": "kitchen",
                                  "language": "en-US",
                                  "capabilities": {
                                    "speaker": true,
                                    "display": false,
                                    "supports_barge_in": true,
                                    "supports_streaming_tts": true
                                  },
                                  "audio_format": {
                                    "encoding": "pcm_s16le",
                                    "sample_rate": 16000,
                                    "channels": 1,
                                    "frame_ms": 20
                                  }
                                }
                                """;
    public const string SessionStart = """
                                       {
                                         "type": "session.start",
                                         "session_id": "uuid-v4",
                                         "timestamp": 123456789
                                       }
                                       """;
    public const string AudioEnd = """
                                   {
                                     "type": "audio.end",
                                     "session_id": "uuid-v4",
                                     "reason": "silence"
                                   }
                                   """;
}