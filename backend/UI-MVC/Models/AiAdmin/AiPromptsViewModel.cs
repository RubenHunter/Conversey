using Conversey.BL.Domain.Ai;

namespace Conversey.UI_MVC.Models.AiAdmin;

public class AiPromptsViewModel
{
    public IReadOnlyList<AiPrompt> Prompts { get; set; } = Array.Empty<AiPrompt>();
    public string SearchQuery { get; set; } = string.Empty;
    public Dictionary<string, string> DefaultDescriptions { get; set; } = new();
}

public class AiPromptEditViewModel
{
    public AiPrompt Prompt { get; set; } = new();
    public AiPrompt DefaultPrompt { get; set; } = new();
    public bool HasDefault { get; set; }
}
