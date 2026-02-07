using AssistantCore.Companion.Messaging;

namespace AssistantCore.Companion;

public class CompanionManager(ICompanionMessageHandler messageHandler)
{
    private List<CompanionApp> _registeredCompanions = [];

    public void RegisterCompanion(CompanionApp app) => _registeredCompanions.Add(app);
    public void UnregisterCompanion(string deviceId) => _registeredCompanions.RemoveAll(a => a.DeviceId == deviceId);

    public void RequestToolApproval(string toolName, string description, string riskLevel)
    {
        var data = new Dictionary<string, string>
        {
            { "request_id", Guid.NewGuid().ToString() },
            { "payload_hash", "TODO" }, // TODO: security and hashing
            { "nonce", Guid.NewGuid().ToString() },
            { "expires_at", DateTime.Now.AddMinutes(5).ToString("O") },
            { "tool_id", toolName },
            { "tool_description", description },
            { "tool_risk_level", riskLevel }
        };
        messageHandler.SendDataMessage("tool_approval", data);
    }
}