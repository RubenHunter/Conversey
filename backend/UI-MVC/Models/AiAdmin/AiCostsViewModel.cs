using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;

namespace Conversey.UI_MVC.Models.AiAdmin;

public class AiCostsViewModel
{
    public AiCostsTimelineSummary Timeline { get; set; } = new();
    public AiCostsSummary Summary { get; set; } = new();
    public IReadOnlyCollection<AiAuditLog> AuditLogs { get; set; } = Array.Empty<AiAuditLog>();
    public IReadOnlyList<string> Workspaces { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Models { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> ModelTypes { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Providers { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Prompts { get; set; } = Array.Empty<string>();
    public AiCostsFilterViewModel Filter { get; set; } = new();
}

public class AiCostsFilterViewModel
{
    public string? WorkspaceId { get; set; }
    public string? ModelName { get; set; }
    public string? ModelType { get; set; }
    public string? ProviderName { get; set; }
    public string? PromptName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int TimelineDays { get; set; } = 30;
}
