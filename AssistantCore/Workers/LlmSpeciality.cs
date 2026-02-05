namespace AssistantCore.Workers;

[Flags]
public enum LlmSpeciality
{
    General = 0x01, 
    Coding = 0x02,
    HomeControl = 0x04,
    
    None = 0x00,
    All = General | Coding | HomeControl
}