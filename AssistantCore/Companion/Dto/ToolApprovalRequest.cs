using System.ComponentModel.DataAnnotations;
using AssistantCore.Companion.Security;

namespace AssistantCore.Companion.Dto;

public class ToolApprovalRequest
{
    [Key] public required string RequestId { get; set; }
    public required string PayloadHash { get; set; }
    public required string Nonce { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ToolData Tool { get; set; }

    public class ToolData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public RiskLevel RiskLevel { get; set; }
    }
}