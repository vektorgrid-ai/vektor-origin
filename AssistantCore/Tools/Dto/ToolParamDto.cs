using System.Text.Json.Serialization;

namespace AssistantCore.Tools.Dto;

public struct ToolParamDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("enum")]
    public string[]? Enum { get; set; }
    
    [JsonPropertyName("required")]
    public bool Required { get; set; }
    
    [JsonPropertyName("default")]
    public object? Default { get; set; }
}