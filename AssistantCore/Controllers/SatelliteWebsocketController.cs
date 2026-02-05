using AssistantCore.Voice;
using Microsoft.AspNetCore.Mvc;

namespace AssistantCore.Controllers;

[ApiController]
[Route("/ws/satellite")]
public class SatelliteWebsocketController(SatelliteManager manager, 
    ILogger<SatelliteConnection> connLogger,
    IHostApplicationLifetime lifetime) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> RegisterSatellite()
    {
        var context = ControllerContext.HttpContext;

        if (!context.WebSockets.IsWebSocketRequest)
            return BadRequest("WebSocket request expected");
        
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();
        var connection = SatelliteConnection.Create(connectionId, socket, connLogger);
        manager.RegisterConnection(connection);
        try
        {
            // Link CancellationToken so the connection is closed when the host is stopping or the request is aborted
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, lifetime.ApplicationStopping);
            await connection.RunAsync(linkedCts.Token);
        }
        finally
        {
            manager.UnregisterConnection(connection);
        }

        return Empty;
    }
}