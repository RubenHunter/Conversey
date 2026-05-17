using Conversey.BL.Domain.Ai;

namespace Conversey.UI_MVC.Models.WorkspaceAi;

public class WorkspaceAiLimitsViewModel
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public AiCostLimit? WorkspaceLimit { get; set; }
    public IReadOnlyList<AiCostLimit> WorkspaceLimitHistory { get; set; } = Array.Empty<AiCostLimit>();
    public List<ProjectLimitEntry> ProjectLimits { get; set; } = new();
    public decimal CurrentWorkspaceCost { get; set; }
    public Dictionary<string, decimal> CurrentProjectCosts { get; set; } = new();
}

public class ProjectLimitEntry
{
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public AiCostLimit? ActiveLimit { get; set; }
    public decimal CurrentCost { get; set; }
    public bool IsOverLimit { get; set; }
}

public class AiCostLimitFormViewModel
{
    public int? Id { get; set; }
    public decimal LimitAmount { get; set; }
    public DateTime PeriodStart { get; set; } = DateTime.UtcNow.Date;
    public DateTime PeriodEnd { get; set; } = DateTime.UtcNow.Date.AddMonths(1);
    public bool IsActive { get; set; } = true;
    public string? WorkspaceId { get; set; }
    public string? ProjectId { get; set; }
}
