using Conversey.BL.Domain.Administration;

namespace Conversey.BL.Administration;

/// <summary>
/// Service interface for retrieving admin statistics and dashboard data.
/// </summary>
public interface IAdminStatsService
{
    /// <summary>
    /// Gets platform-wide dashboard statistics for Conversey Admins.
    /// </summary>
    Task<DashboardStatsDto> GetPlatformDashboardAsync();

    /// <summary>
    /// Gets workspace-specific dashboard statistics for Workspace Admins.
    /// </summary>
    /// <param name="workspaceId">The workspace ID to get stats for.</param>
    Task<DashboardStatsDto> GetWorkspaceDashboardAsync(Conversey.BL.Domain.Common.Slug workspaceId);

    /// <summary>
    /// Gets platform-wide statistics.
    /// </summary>
    Task<PlatformStatsDto> GetPlatformStatsAsync();

    /// <summary>
    /// Gets workspace-specific statistics.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    Task<WorkspaceStatsDto> GetWorkspaceStatsAsync(Conversey.BL.Domain.Common.Slug workspaceId);

    /// <summary>
    /// Gets usage trend data for a specific period.
    /// </summary>
    /// <param name="admin">The admin requesting the data.</param>
    /// <param name="period">The time period (7d, 1m, total).</param>
    Task<UsageTrendDto> GetUsageTrendAsync(Admin admin, string period = "7d");

    /// <summary>
    /// Gets comparison data for the comparison widget.
    /// </summary>
    /// <param name="admin">The admin requesting the data.</param>
    Task<ComparisonWidgetDto> GetComparisonDataAsync(Admin admin);

    /// <summary>
    /// Gets quick links data for the quick links widget.
    /// </summary>
    /// <param name="admin">The admin requesting the data.</param>
    Task<QuickLinksWidgetDto> GetQuickLinksDataAsync(Admin admin);

    /// <summary>
    /// Gets engagement metrics for the engagement widget.
    /// </summary>
    /// <param name="admin">The admin requesting the data.</param>
    Task<EngagementWidgetDto> GetEngagementDataAsync(Admin admin);
}

/// <summary>
/// DTO for complete dashboard statistics.
/// </summary>
public class DashboardStatsDto
{
    public PlatformStatsDto? PlatformStats { get; set; }
    public WorkspaceStatsDto? WorkspaceStats { get; set; }
    public List<StatWidgetDto> StatWidgets { get; set; } = new();
    public List<ChartWidgetDto> MainCharts { get; set; } = new();
    public List<ActionWidgetDto> ActionWidgets { get; set; } = new();
    public List<ProgressWidgetDto> ProgressWidgets { get; set; } = new();
    public ComparisonWidgetDto? ComparisonWidget { get; set; }
    public QuickLinksWidgetDto? QuickLinksWidget { get; set; }
    public EngagementWidgetDto? EngagementWidget { get; set; }
    public ChartWidgetDto? UsageTrendChart { get; set; }
}

/// <summary>
/// DTO for platform-wide statistics.
/// </summary>
public class PlatformStatsDto
{
    public int TotalWorkspaces { get; set; }
    public int TotalProjects { get; set; }
    public int TotalUsers { get; set; }
    public int TotalIdeas { get; set; }
    public int ActiveAiProviders { get; set; }
    public DateTime LastActivityDate { get; set; }
}

/// <summary>
/// DTO for workspace-specific statistics.
/// </summary>
public class WorkspaceStatsDto
{
    public Conversey.BL.Domain.Common.Slug WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public int TotalProjects { get; set; }
    public int TotalTopics { get; set; }
    public int TotalYouths { get; set; }
    public int TotalIdeas { get; set; }
    public int ActiveUsers { get; set; }
    public DateTime LastActivityDate { get; set; }
}

/// <summary>
/// DTO for stat widget.
/// </summary>
public class StatWidgetDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? SubLabel { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// DTO for chart widget.
/// </summary>
public class ChartWidgetDto
{
    public string Title { get; set; } = string.Empty;
    public string CanvasId { get; set; } = System.Guid.NewGuid().ToString();
    public string Type { get; set; } = "line";
    public object? Data { get; set; }
    public object? Options { get; set; }
    public List<PeriodDto> Periods { get; set; } = new();
    public string ActivePeriod { get; set; } = "1m";
    public Dictionary<string, object> PeriodDatasets { get; set; } = new();
}

/// <summary>
/// DTO for period filter.
/// </summary>
public class PeriodDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for action widget.
/// </summary>
public class ActionWidgetDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string ActionText { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
    public string? ModalTarget { get; set; }
    public bool IsModal { get; set; }
}

/// <summary>
/// DTO for progress widget.
/// </summary>
public class ProgressWidgetDto
{
    public string Label { get; set; } = string.Empty;
    public int Percentage { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// DTO for comparison widget.
/// </summary>
public class ComparisonWidgetDto
{
    public string Title { get; set; } = "Active projects";
    public string SubTitle { get; set; } = "All projects";
    public string TitleUrl { get; set; } = "/admin/projects";
    public string SubTitleUrl { get; set; } = "/admin/projects?filter=all";
    public List<ComparisonItemDto> Items { get; set; } = new();
    public List<ComparisonItemDto> AllItems { get; set; } = new();
}

/// <summary>
/// DTO for comparison item.
/// </summary>
public class ComparisonItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = "primary";
    public string? LegendIcon { get; set; }
}

/// <summary>
/// DTO for quick links widget.
/// </summary>
public class QuickLinksWidgetDto
{
    public string? Title { get; set; }
    public List<QuickLinkItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for quick link item.
/// </summary>
public class QuickLinkItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconBgHex { get; set; } = string.Empty;
    public string IconFgHex { get; set; } = string.Empty;
    public string? ModalTarget { get; set; }
    public string NavigateUrl { get; set; } = string.Empty;
    public bool IsHealthCheck { get; set; }
    public bool IsModal => !string.IsNullOrEmpty(ModalTarget);
}

/// <summary>
/// DTO for engagement widget.
/// </summary>
public class EngagementWidgetDto
{
    public string Title { get; set; } = "Weekly Engagement Rate";
    public int GaugePercentage { get; set; }
    public string GaugeLabel { get; set; } = "Full Completion Rate";
    public string GaugeColor { get; set; } = "text-yellow-400";
    public List<EngagementBarDto> Bars { get; set; } = new();
}

/// <summary>
/// DTO for engagement progress bar.
/// </summary>
public class EngagementBarDto
{
    public string Label { get; set; } = string.Empty;
    public string SubLabel { get; set; } = string.Empty;
    public int Current { get; set; }
    public int Max { get; set; } = 100;
    public string DisplayValue { get; set; } = string.Empty;
    public string Color { get; set; } = "bg-green-500";
}

/// <summary>
/// DTO for usage trend data.
/// </summary>
public class UsageTrendDto
{
    public string Title { get; set; } = "Usage Trend";
    public string Type { get; set; } = "line";
    public List<string> Labels { get; set; } = new();
    public List<int> Values { get; set; } = new();
    public string Period { get; set; } = "7d";
}
