using System.Net.WebSockets;

namespace CoreTests.Satellite;

public class SatelliteSocketTests(TestAssistantApp factory)
    : IClassFixture<TestAssistantApp>
{
    private const string HelloMessage =
        "{\n  \"type\": \"hello\",\n  \"protocol_version\": 1,\n  \"mic_id\": \"kitchen_satellite\",\n  \"area\": \"kitchen\",\n  \"language\": \"en-US\",\n  \"capabilities\": {\n    \"speaker\": true,\n    \"display\": false,\n    \"supports_barge_in\": true,\n    \"supports_streaming_tts\": true\n  },\n  \"audio_format\": {\n    \"encoding\": \"pcm_s16le\",\n    \"sample_rate\": 16000,\n    \"channels\": 1,\n    \"frame_ms\": 20\n  }\n}";

    private const string SessionStartMessage =
        "{\n  \"type\": \"session.start\",\n  \"session_id\": \"uuid-v4\",\n  \"timestamp\": 123456789\n}";

    private const string AudioEndMessage =
        "{\n  \"type\": \"audio.end\",\n  \"session_id\": \"uuid-v4\",\n  \"reason\": \"silence\"\n}";

    private const int AudioFrameSizeBytes = 640; // 20ms of PCM 16kHz mono audio with 16-bit samples (values from HelloMessage)
    
    [Fact]
    public async Task Hello_ProducesHelloAck()
    {
        await using var client = await SatelliteTestClient.ConnectAsync(factory);

        await client.SendJsonAsync(SatelliteProtocol.Hello);

        var msg = await client.ReceiveJsonAsync();
        Assert.Equal("hello.ack", msg.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task SessionStart_ProducesSessionAck()
    {
        await using var client = await SatelliteTestClient.ConnectAsync(factory);

        await client.SendJsonAsync(SatelliteProtocol.Hello);
        await client.ReceiveJsonAsync();

        await client.SendJsonAsync(SatelliteProtocol.SessionStart);

        var msg = await client.ReceiveJsonAsync();
        Assert.Equal("session.ack", msg.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public async Task AudioEnd_TriggersTtsStart()
    {
        await using var client = await SatelliteTestClient.ConnectAsync(factory);

        await client.SendJsonAsync(SatelliteProtocol.Hello);
        await client.ReceiveJsonAsync();

        await client.SendJsonAsync(SatelliteProtocol.SessionStart);
        await client.ReceiveJsonAsync();

        await client.SendRandomAudioAsync(
            SatelliteProtocol.AudioFrameSize, 50);

        await client.SendJsonAsync(SatelliteProtocol.AudioEnd);

        var msg = await client.ReceiveJsonAsync();
        Assert.Equal("tts.start", msg.RootElement.GetProperty("type").GetString());
    }
    
    [Fact]
    public async Task FullLoop_EmitsAudioAndTtsEnd()
    {
        await using var client = await SatelliteTestClient.ConnectAsync(factory);

        await client.SendJsonAsync(SatelliteProtocol.Hello);
        await client.ReceiveJsonAsync();

        await client.SendJsonAsync(SatelliteProtocol.SessionStart);
        await client.ReceiveJsonAsync();

        await client.SendRandomAudioAsync(
            SatelliteProtocol.AudioFrameSize, 50);

        await client.SendJsonAsync(SatelliteProtocol.AudioEnd);

        var start = await client.ReceiveJsonAsync();
        Assert.Equal("tts.start", start.RootElement.GetProperty("type").GetString());

        var buffer = new byte[SatelliteProtocol.AudioFrameSize];

        for (int i = 0; i < SatelliteProtocol.TtsFrames; i++)
        {
            var result = await client.ReceiveBinaryAsync(buffer);

            Assert.Equal(WebSocketMessageType.Binary, result.MessageType);
            Assert.Equal(SatelliteProtocol.AudioFrameSize, result.Count);
        }

        var end = await client.ReceiveJsonAsync();
        Assert.Equal("tts.end", end.RootElement.GetProperty("type").GetString());
    }
}
