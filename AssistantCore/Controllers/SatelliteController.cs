using AssistantCore.Voice;
using Microsoft.AspNetCore.Mvc;

namespace AssistantCore.Controllers;

[ApiController]
[Route("/satellite")]
public class SatelliteController(SatelliteManager manager, 
    ILogger<SatelliteConnection> connLogger,
    IHostApplicationLifetime lifetime) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllSatellites()
    {
        var satellites = manager.GetActiveConnections().Select(c => new
        {
            connection_id = c.ConnectionId,
            connection_state = c.State.ToString(),
            device_name = c.SatelliteInfo?.SatelliteId
        });
        return Ok(satellites);
    }
}