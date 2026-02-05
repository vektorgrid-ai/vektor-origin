using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

internal sealed class SatelliteTestClient : IAsyncDisposable
{
    private readonly WebSocket _socket;

    private SatelliteTestClient(WebSocket socket)
    {
        _socket = socket;
    }

    public static async Task<SatelliteTestClient> ConnectAsync(
        WebApplicationFactory<Program> factory)
    {
        var client = factory.Server.CreateWebSocketClient();
        var socket = await client.ConnectAsync(
            new Uri("ws://localhost/ws/satellite"),
            CancellationToken.None);

        return new SatelliteTestClient(socket);
    }

    public async Task SendJsonAsync(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        await _socket.SendAsync(
            bytes,
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public async Task<JsonDocument> ReceiveJsonAsync()
    {
        var buffer = new byte[4096];
        var result = await _socket.ReceiveAsync(buffer, CancellationToken.None);

        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
        return JsonDocument.Parse(text);
    }

    public async Task<WebSocketReceiveResult> ReceiveBinaryAsync(byte[] buffer)
    {
        return await _socket.ReceiveAsync(buffer, CancellationToken.None);
    }

    public async Task SendRandomAudioAsync(int frameSize, int frames)
    {
        var buf = new byte[frameSize];
        for (int i = 0; i < frames; i++)
        {
            Random.Shared.NextBytes(buf);
            await _socket.SendAsync(buf, WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _socket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Test done",
            CancellationToken.None);
    }
}