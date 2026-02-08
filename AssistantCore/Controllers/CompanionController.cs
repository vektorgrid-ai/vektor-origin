using System.Text.Json;
using AssistantCore.Companion;
using AssistantCore.Companion.Dto;
using AssistantCore.Companion.Security;
using Microsoft.AspNetCore.Mvc;

namespace AssistantCore.Controllers;

[ApiController]
[Route("companion")]
public class CompanionController(
    CompanionManager manager,
    ILogger<CompanionController> logger) : ControllerBase
{
    [HttpPost("enroll")]
    public IActionResult Enroll([FromBody] EnrollmentRequest req)
    {
        var device = new CompanionDevice
        {
            DeviceId = Guid.NewGuid().ToString(),
            PublicKey = req.PublicKey,
            FirebaseToken = req.FirebaseToken,
            DeviceName = req.DeviceName,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        manager.RegisterCompanion(device);

        logger.LogInformation("New companion enrolled: {DeviceName} ({DeviceId})", device.DeviceName, device.DeviceId);
        return Ok(new { device_id = device.DeviceId, status = "PENDING_APPROVAL" });
    }
    
    [HttpPost("new_token")]
    public IActionResult RegisterNewToken([FromBody] NewTokenRequest request)
    {
        if (!ValidateDeviceId(request.DeviceId, false, out var device, out var errorResult)) return errorResult!;
        device = device!;
        
        manager.GetCompanion(request.DeviceId)?.FirebaseToken = request.Token;
        logger.LogDebug("Updated Firebase token for device {DeviceId}", request.DeviceId);
        return Ok("Token updated successfully");
    }

    [HttpPost("answer_request")]
    public IActionResult AnswerRequest([FromBody] ToolAnswer request)
    {
        if (!ValidateDeviceId(request.DeviceId, true, out var device, out var errorResult)) return errorResult!;
        device = device!;
        
        // TODO: verify request, check signature, implement approval logic
        return Ok("Answer received");
    }

    [HttpGet("send_test_tool")]
    public IActionResult SendTestTool()
    {
        manager.RequestToolApproval("TestTool", "This is a test tool for demonstration purposes.", RiskLevel.None);
        return Ok();
    }
    [HttpGet("send_test_notification")]
    public IActionResult SendTestNotification()
    {
        manager.SendNotification("Test Notification", "This is a test notification sent to all approved companion devices.");
        return Ok();
    }

    [HttpPost("temp/approve")]
    public IActionResult TempApprove([FromBody] object body)
    {
        // This is a temporary endpoint for testing. In production, approval should be done through a secure admin interface (TODO)
        JsonDocument doc = JsonDocument.Parse(body.ToString() ?? string.Empty);
        var deviceId = doc.RootElement.GetProperty("device_id").GetString();
        if (string.IsNullOrEmpty(deviceId)) return BadRequest("Invalid or missing device_id");

        if (!ValidateDeviceId(deviceId, false, out var device, out var errorResult)) return errorResult!;
        
        manager.ApproveCompanion(deviceId, false);
        logger.LogInformation("Device {DeviceName} ({DeviceId}) approved", device!.DeviceName, device.DeviceId);
        return Ok();
    }

    private bool ValidateDeviceId(string deviceId, bool requireApproval, out CompanionDevice? device, out IActionResult? errorResult)
    {
        errorResult = null;
        device = manager.GetCompanion(deviceId);
        if (device == null)
        {
            errorResult = BadRequest("Unknown device");
            return false;
        }

        if (!device.IsApproved && requireApproval)
        {
            errorResult = BadRequest("Device not approved. Ask administrator for approval.");
            return false;
        }

        return true;
    }
}