using AssistantCore.Companion.Security;
using AssistantCore.Workers;

namespace AssistantCore.Tools;

public class LlmToolAttribute : Attribute
{
    public string ToolName { get; }
    public string Description { get; }
    public LlmSpeciality Speciality { get; }
    public RiskLevel RiskLevel { get; }
    public LlmToolAttribute(
        string toolName, 
        string description, 
        LlmSpeciality speciality = LlmSpeciality.General,
        RiskLevel riskLevel = RiskLevel.None) 
    {
        ToolName = toolName;
        Description = description;
        Speciality = speciality;
        RiskLevel = riskLevel;
    }
}