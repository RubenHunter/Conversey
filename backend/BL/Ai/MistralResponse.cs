using System.Text.Json.Serialization;

namespace Conversey.BL.Ai;

public class MistralAiResponse
{
    [JsonPropertyName("choices")]
    public Choice[] Choices { get; set; }
    public string Id { get; set; }
    public long Created { get; set; }
    public string Model { get; set; }
    public Usage Usage { get; set; }
    public string Object { get; set; }
}

public class Choice
{
    [JsonPropertyName("message")]
    public Message Message { get; set; }
    public int Index { get; set; }
    public string FinishReason { get; set; }
}

public class Message
{
    public string Role { get; set; }
    public object ToolCalls { get; set; }
    
    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int TotalTokens { get; set; }
    public int CompletionTokens { get; set; }
}
