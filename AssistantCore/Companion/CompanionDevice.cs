namespace AssistantCore.Companion;

public class CompanionDevice
{
    public required string DeviceId { get; set; }
    public string DeviceName { get; set; } = "Unnamed Device";
    public string? FirebaseToken { get; set; }
    public bool IsApproved { get; set; }
    public required string PublicKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public CompanionPermissions Permissions { get; set; }

    public struct CompanionPermissions
    {
        public bool CanApproveTools { get; set; }
    }
}