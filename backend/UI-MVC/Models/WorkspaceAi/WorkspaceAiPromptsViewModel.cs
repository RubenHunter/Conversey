using Conversey.BL.Domain.Ai;

namespace Conversey.UI_MVC.Models.WorkspaceAi;

public class WorkspaceAiPromptsViewModel
{
    public string WorkspaceId { get; set; } = string.Empty;
    public IReadOnlyList<AiPrompt> Prompts { get; set; } = Array.Empty<AiPrompt>();
    public IReadOnlyList<BL.Domain.Administration.Project> Projects { get; set; } = Array.Empty<BL.Domain.Administration.Project>();
    public string SelectedProjectId { get; set; } = string.Empty;
    public string SearchQuery { get; set; } = string.Empty;
}

public class WorkspaceAiPromptEditViewModel
{
    public AiPrompt Prompt { get; set; } = new();
    public AiPrompt DefaultPrompt { get; set; } = new();
    public bool HasDefault { get; set; }
    public string WorkspaceId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}
