namespace Conversey.BL.Ai;

public class AiCompletionResult
{
    public string Content { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
}

public class AiModerationResult
{
    public bool Flagged { get; set; }
    public IReadOnlyDictionary<string, bool> Categories { get; set; } = new Dictionary<string, bool>();
}
