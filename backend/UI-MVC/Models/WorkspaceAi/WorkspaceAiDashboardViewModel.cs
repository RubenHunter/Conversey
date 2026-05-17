using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;

namespace Conversey.UI_MVC.Models.WorkspaceAi;

public class WorkspaceAiDashboardViewModel
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public decimal WorkspaceTotalCost { get; set; }
    public AiCostLimit? WorkspaceLimit { get; set; }
    public bool IsWorkspaceOverLimit { get; set; }
    public List<ProjectCostEntry> ProjectCosts { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class ProjectCostEntry
{
    public string ProjectId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int CallCount { get; set; }
    public AiCostLimit? Limit { get; set; }
    public bool IsOverLimit { get; set; }
}
