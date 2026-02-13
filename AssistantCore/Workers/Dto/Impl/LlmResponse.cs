using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssistantCore.Workers.Dto.Impl;

public record LlmResponse : WorkerResponse<LlmOutput>
{
    public LlmResponse(string requestId, WorkerUsage usage, LlmOutput output, string? error)
        : base(requestId, usage, output, error)
    {
    }
}

public record LlmOutput
{
    [JsonPropertyName("text")] public string Text { get; init; }
    [JsonPropertyName("tool_calls")] public List<LlmToolCall> ToolCalls { get; set; }
    
    public LlmOutput(string text, List<LlmToolCall> toolCalls)
    {
        Text = text;
        ToolCalls = toolCalls;
    }
}

public record LlmToolCall
{
    // {"function":{"name":"get_weather","arguments":{"unit":"Celsius","location":"office"}}}
    [JsonPropertyName("function")] public LlmToolFunction Function { get; set; }
    
    public LlmToolCall(LlmToolFunction function)
    {
        Function = function;
    }
    
    public record LlmToolFunction
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("arguments")] public JsonElement Arguments { get; set; }
        
        public LlmToolFunction(string name, JsonElement arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }
}
