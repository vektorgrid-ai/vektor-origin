using AssistantCore.Companion.Messaging;
using AssistantCore.Companion.Security;

namespace AssistantCore.Companion;

public class CompanionManager(ICompanionMessageHandler messageHandler)
{
    private readonly List<CompanionDevice> _registeredCompanions = [];
    public List<CompanionDevice> ApprovedCompanions => _registeredCompanions.Where(c => c.IsApproved).ToList();

    public void RegisterCompanion(CompanionDevice device) => _registeredCompanions.Add(device);
    public void UnregisterCompanion(string deviceId) => _registeredCompanions.RemoveAll(a => a.DeviceId == deviceId);

    public CompanionDevice? GetCompanion(string deviceId) =>
        _registeredCompanions.FirstOrDefault(c => c.DeviceId == deviceId);

    public void RequestToolApproval(string toolName, string description, RiskLevel riskLevel)
    {
        var data = new Dictionary<string, string>
        {
            { "request_id", Guid.NewGuid().ToString() },
            { "payload_hash", "TODO" }, // TODO: security and hashing
            { "nonce", Guid.NewGuid().ToString() },
            { "expires_at", DateTime.Now.AddMinutes(5).ToString("O") },
            { "tool_id", toolName },
            { "tool_description", description },
            { "tool_risk_level", riskLevel.ToString() }
        };
        messageHandler.SendDataMessageToDevices(data, ApprovedCompanions.ToArray());
    }

    public void ApproveCompanion(string deviceId, bool sendConfirmation = true)
    {
        var device = GetCompanion(deviceId);
        if (device == null) return;
        
        device.IsApproved = true;
        if (sendConfirmation)
            messageHandler.SendNotificationToDevices("Device Approved",
            $"Your device '{device.DeviceName}' has been approved. You can now receive notifications and tool approval requests.", 
            device);
    }

    public void SendNotification(string title, string message)
    {
        messageHandler.SendNotificationToDevices(title, message, ApprovedCompanions.ToArray());
    }
}