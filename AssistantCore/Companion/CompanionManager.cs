using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using AssistantCore.Companion.Dto;
using AssistantCore.Companion.Messaging;

namespace AssistantCore.Companion;

public class CompanionManager(ICompanionMessageHandler messageHandler)
{
    public static readonly TimeSpan ApprovalTimeout = TimeSpan.FromMinutes(5);
    
    public readonly List<CompanionDevice> RegisteredCompanions = [];
    public List<CompanionDevice> ApprovedCompanions => RegisteredCompanions.Where(c => c.IsApproved).ToList();
    
    // TODO: put in central storage
    public readonly List<ToolApprovalRequest> ToolApprovals = [];
    
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingApprovals = new();

    public void RegisterCompanion(CompanionDevice device) => RegisteredCompanions.Add(device);
    public void UnregisterCompanion(string deviceId) => RegisteredCompanions.RemoveAll(a => a.DeviceId == deviceId);

    public CompanionDevice? GetCompanion(string deviceId) =>
        RegisteredCompanions.FirstOrDefault(c => c.DeviceId == deviceId);

    public Task<bool> RequestToolApprovalAsync(ToolApprovalRequest.ToolData toolData)
    {
        if (ApprovedCompanions.Count <= 0) return Task.FromResult(false);
        
        var tcs = new TaskCompletionSource<bool>();
        var hash = HashPayload(toolData);
        var hashBase64 = Convert.ToBase64String(hash);
        var request = new ToolApprovalRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            PayloadHash = hashBase64,
            Nonce = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.Now.Add(ApprovalTimeout),
            Tool = toolData
        };
        
        _pendingApprovals.TryAdd(request.RequestId, tcs);
        
        var data = new Dictionary<string, string>
        {
            { "request_id", request.RequestId },
            { "payload_hash", request.PayloadHash },
            { "nonce",request.Nonce },
            { "expires_at", request.ExpiresAt.ToString("O") },
            { "tool_name", request.Tool.Name },
            { "tool_description", request.Tool.Description },
            { "tool_risk_level", request.Tool.RiskLevel.ToString() }
        };
        
        ToolApprovals.Add(request);
        messageHandler.SendDataMessageToDevices(data, ApprovedCompanions.ToArray());
        
        return tcs.Task;
    }

    public void HandleToolResponse(string requestId, bool isApproved)
    {
        if (_pendingApprovals.TryRemove(requestId, out var tcs))
        {
            tcs.TrySetResult(isApproved);
        }
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

    public bool VerifyHash(ToolApprovalRequest request, string payloadHash)
    {
        var expectedHash = HashPayload(request.Tool);
        var expectedHashBase64 = Convert.ToBase64String(expectedHash);

        return expectedHashBase64 == payloadHash;
    }

    private byte[] HashPayload(ToolApprovalRequest.ToolData data)
    {
        string payloadString = $"{data.Name}|{data.Description}|{data.RiskLevel}";
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(Encoding.UTF8.GetBytes(payloadString));
    }
}