using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Administration;

namespace Conversey.UI_MVC.Models.WorkspaceAi;

public class WorkspaceAiCostsViewModel
{
    public string WorkspaceId { get; set; } = string.Empty;
    public AiCostsTimelineSummary Timeline { get; set; } = new();
    public AiCostsSummary Summary { get; set; } = new();
    public IReadOnlyCollection<AiAuditLog> AuditLogs { get; set; } = Array.Empty<AiAuditLog>();
    public IReadOnlyList<Project> Projects { get; set; } = Array.Empty<Project>();
    public IReadOnlyList<string> Models { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> ModelTypes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Providers { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Prompts { get; set; } = Array.Empty<string>();
    public WorkspaceAiCostsFilterViewModel Filter { get; set; } = new();
}

public class WorkspaceAiCostsFilterViewModel
{
    public string? ProjectId { get; set; }
    public string? ModelName { get; set; }
    public string? ModelType { get; set; }
    public string? ProviderName { get; set; }
    public string? PromptName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int TimelineDays { get; set; } = 30;
}
