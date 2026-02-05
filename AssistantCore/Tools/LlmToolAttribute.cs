using AssistantCore.Workers;

namespace AssistantCore.Tools;

public class LlmToolAttribute : Attribute
{
    public string ToolName { get; }
    public string Description { get; }
    public LlmSpeciality Speciality { get; }
    public LlmToolAttribute(string toolName, string description, LlmSpeciality speciality = LlmSpeciality.General)
    {
        ToolName = toolName;
        Description = description;
        Speciality = speciality;
    }
}