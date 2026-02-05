using System.Text.Json.Serialization;

namespace AssistantCore.Chat;

public sealed class ChatContext
{
    [JsonPropertyName("chat_id")]
    public Guid ChatId { get; init; }
    [JsonPropertyName("events")]
    public List<ChatEvent> Events { get; init; } = [];
}

[JsonPolymorphic]
[JsonDerivedType(typeof(UserMessage), typeDiscriminator: "user")]
[JsonDerivedType(typeof(AssistantMessage), typeDiscriminator: "assistant")]
[JsonDerivedType(typeof(ToolCall), typeDiscriminator: "tool_call")]
[JsonDerivedType(typeof(ToolResult), typeDiscriminator: "tool_result")]
public abstract record ChatEvent([property: JsonPropertyName("timestamp")] DateTime Timestamp)
{
    protected ChatEvent() : this(DateTime.UtcNow) { }
}

public record UserMessage([property: JsonPropertyName("text")] string Text) : ChatEvent;
public record AssistantMessage([property: JsonPropertyName("text")] string Text) : ChatEvent;
public record ToolCall([property: JsonPropertyName("tool_name")] string ToolName, [property: JsonPropertyName("json_args")] string JsonArgs) : ChatEvent;
public record ToolResult([property: JsonPropertyName("tool_name")] string ToolName, [property: JsonPropertyName("json_result")] string JsonResult) : ChatEvent;