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
            device_name = c.SatelliteInfo?.SatelliteId,
            audio_format = new
            {
                encoding = c.SatelliteInfo?.AudioFormat.Encoding,
                channels = c.SatelliteInfo?.AudioFormat.Channels,
                sample_rate = c.SatelliteInfo?.AudioFormat.SampleRate,
                frame_ms = c.SatelliteInfo?.AudioFormat.FrameMs
            },
            area = c.SatelliteInfo?.Area,
            version = c.SatelliteInfo?.ProtocolVersion,
            capabilities = new
            {
                has_display = c.SatelliteInfo?.Capabilities.Display,
                has_speaker = c.SatelliteInfo?.Capabilities.Display,
                supports_streaming = c.SatelliteInfo?.Capabilities.SupportsStreamingTts
            }
        });
        return Ok(satellites);
    }
}