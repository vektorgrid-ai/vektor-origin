using System.Text.Json.Serialization;

namespace AssistantCore.Voice.Dto;

public class SatelliteDto
{
    [JsonPropertyName("type")] public string Type { get; set; }
}