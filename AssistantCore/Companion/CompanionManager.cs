using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using AssistantCore.Companion.Dto;
using AssistantCore.Companion.Messaging;
using AssistantCore.Database;

namespace AssistantCore.Companion;

public class CompanionManager(ICompanionMessageHandler messageHandler, IServiceProvider serviceProvider)
{
    public static readonly TimeSpan ApprovalTimeout = TimeSpan.FromMinutes(5);

    public List<CompanionDevice> GetRegisteredCompanions()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        return dbContext.Companions.ToList();
    }

    public List<CompanionDevice> GetApprovedCompanions()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        return dbContext.Companions.Where(c => c.IsApproved).ToList();
    }

    public List<ToolApprovalRequest> GetToolApprovals()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        return dbContext.ToolApprovals.ToList();
    }
    
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingApprovals = new();

    /// <summary>
    /// Inserts a new companion into the database if one doesn't exist already
    /// </summary>
    /// <param name="device">The CompanionDevice to try to insert</param>
    /// <returns>The companion device that has either been inserted or already existed with the same ID as the requested device</returns>
    public CompanionDevice TryRegisterCompanion(CompanionDevice device)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        var devices = dbContext.Companions.Where(c => c.DeviceId == device.DeviceId).ToList();

        if (devices.Count == 0)
        {
            dbContext.Companions.Add(device);
            dbContext.SaveChanges();
            return device;
        }

        return devices.Single();
    }

    public void UnregisterCompanion(string deviceId)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        var devices = dbContext.Companions.Where(c => c.DeviceId == deviceId).ToList();
        if (devices.Count != 0)
        {
            dbContext.Companions.RemoveRange(devices);
            dbContext.SaveChanges();
        }
    }

    public CompanionDevice? GetCompanion(string deviceId)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        return dbContext.Companions.FirstOrDefault(c => c.DeviceId == deviceId);
    }

    public Task<bool> RequestToolApprovalAsync(ToolApprovalRequest.ToolData toolData)
    {
        var approvedCompanions = GetApprovedCompanions();
        if (approvedCompanions.Count <= 0) return Task.FromResult(false);
        
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
        
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
            dbContext.ToolApprovals.Add(request);
            dbContext.SaveChanges();
        }

        messageHandler.SendDataMessageToDevices(data, approvedCompanions.ToArray());
        
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
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        
        var device = dbContext.Companions.FirstOrDefault(c => c.DeviceId == deviceId);
        if (device == null) return;
        
        device.IsApproved = true;
        dbContext.SaveChanges();
        
        if (sendConfirmation)
            messageHandler.SendNotificationToDevices("Device Approved",
            $"Your device '{device.DeviceName}' has been approved. You can now receive notifications and tool approval requests.", 
            device);
    }

    public void SendNotification(string title, string message)
    {
        var approvedCompanions = GetApprovedCompanions();
        messageHandler.SendNotificationToDevices(title, message, approvedCompanions.ToArray());
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
