using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.UI_MVC.Models.Admin;

/// <summary>
/// ViewModel for the unified admin dashboard.
/// Used by both Conversey and Workspace admin dashboards.
/// </summary>
public class DashboardViewModel
{
    /// <summary>
    /// The title displayed at the top of the dashboard page.
    /// </summary>
    public string PageTitle { get; set; } = "Dashboard";

    /// <summary>
    /// The description displayed below the page title.
    /// </summary>
    public string PageDescription { get; set; } = "Manage your administrative area.";

    /// <summary>
    /// The type of admin dashboard: "conversey" or "workspace".
    /// </summary>
    public string AdminType { get; set; } = "unknown";

    /// <summary>
    /// The name of the workspace (for Workspace Admin dashboards).
    /// </summary>
    public string? WorkspaceName { get; set; }

    /// <summary>
    /// The ID of the workspace (for Workspace Admin dashboards).
    /// </summary>
    public string? WorkspaceId { get; set; }

    /// <summary>
    /// Top navigation cards (role-dependent).
    /// </summary>
    public List<NavCardViewModel> NavCards { get; set; } = new();

    /// <summary>
    /// Left widget: Comparison widget (workspace/project counts).
    /// </summary>
    public ComparisonWidgetViewModel? ComparisonWidget { get; set; }

    /// <summary>
    /// Center widget: Quick links (grouped link-list container).
    /// </summary>
    public QuickLinksWidgetViewModel? QuickLinksWidget { get; set; }

    /// <summary>
    /// Right widget: Engagement metrics (circular gauge + progress bars).
    /// </summary>
    public EngagementWidgetViewModel? EngagementWidget { get; set; }

    /// <summary>
    /// Full-width chart: Usage trend with period tabs.
    /// </summary>
    public ChartWidgetViewModel? UsageTrendChart { get; set; }

    /// <summary>
    /// Pre-serialized JSON for the usage trend chart (reused by TypeScript).
    /// </summary>
    public string UsageTrendJson { get; set; } = "[]";

    /// <summary>
    /// Additional stat widgets for future expansion.
    /// </summary>
    public List<StatWidgetViewModel> StatWidgets { get; set; } = new();
}

public static class DashboardViewModelExtensions
{
    public static bool IsConverseyDashboard(this DashboardViewModel model) =>
        model.AdminType == "conversey";

    public static bool IsWorkspaceDashboard(this DashboardViewModel model) =>
        model.AdminType == "workspace";
}

/// <summary>
/// ViewModel for navigation cards (top row building blocks).
/// NOT widgets - these navigate directly to admin sections.
/// </summary>
public class NavCardViewModel
{
    /// <summary>
    /// The title displayed on the card.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description displayed below the title.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The icon to display (emoji or icon class).
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// The URL to navigate to when clicked.
    /// </summary>
    public string NavigateUrl { get; set; } = string.Empty;

    /// <summary>
    /// The background color class for the icon container (e.g., "bg-primary/10").
    /// </summary>
    public string IconBackground { get; set; } = "bg-primary/10";

    /// <summary>
    /// The text/color class for the icon (e.g., "text-primary").
    /// </summary>
    public string IconColor { get; set; } = "text-primary";
}

/// <summary>
/// ViewModel for the Comparison Widget (radial circle comparison).
/// </summary>
public class ComparisonWidgetViewModel
{
    /// <summary>
    /// Unique ID for DOM targeting.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The main title (clickable).
    /// </summary>
    public string Title { get; set; } = "Active projects";

    /// <summary>
    /// The subtitle (clickable, faded).
    /// </summary>
    public string SubTitle { get; set; } = "All projects";

    /// <summary>
    /// URL for the title link.
    /// </summary>
    public string TitleUrl { get; set; } = "/admin/projects";

    /// <summary>
    /// URL for the subtitle link.
    /// </summary>
    public string SubTitleUrl { get; set; } = "/admin/projects?filter=all";

    /// <summary>
    /// Label for the primary toggle button (e.g. "All" or "Projects").
    /// </summary>
    public string ToggleLabel { get; set; } = "All";

    /// <summary>
    /// Label for the secondary toggle button (e.g. "Active" or "Youths").
    /// </summary>
    public string ToggleSecondaryLabel { get; set; } = "Active";

    /// <summary>
    /// Items for the primary/active mode (e.g., active projects).
    /// </summary>
    public List<ComparisonItemViewModel> Items { get; set; } = new();

    /// <summary>
    /// Items for the secondary/all mode (e.g., all projects). When non-empty,
    /// the Title/SubTitle buttons act as a toggle switch between the two datasets.
    /// </summary>
    public List<ComparisonItemViewModel> AllItems { get; set; } = new();

    /// <summary>
    /// Widget size for layout purposes.
    /// </summary>
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

/// <summary>
/// ViewModel for a single comparison item.
/// </summary>
public class ComparisonItemViewModel
{
    /// <summary>
    /// The label for this item (e.g., "Nmbs", "Stad Stabroek").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The numeric value for comparison.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// The color theme for this item (e.g., "primary", "secondary", "accent").
    /// </summary>
    public string Color { get; set; } = "primary";

    /// <summary>
    /// The URL to navigate to when clicking this item's circle.
    /// </summary>
    public string? NavigateUrl { get; set; }

    /// <summary>
    /// Optional icon for the legend.
    /// </summary>
    public string? LegendIcon { get; set; }
}

/// <summary>
/// ViewModel for the Quick Links Widget (grouped list container).
/// </summary>
public class QuickLinksWidgetViewModel
{
    /// <summary>
    /// Optional header title (e.g., "Links").
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The list of quick link items.
    /// </summary>
    public List<QuickLinkItemViewModel> Items { get; set; } = new();

    /// <summary>
    /// Widget size for layout purposes.
    /// </summary>
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

/// <summary>
/// ViewModel for a single quick link item.
/// </summary>
public class QuickLinkItemViewModel
{
    /// <summary>
    /// The title displayed for this link.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description displayed below the title.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The icon to display (emoji or icon class).
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Hex color for icon background, e.g. "#FEF9C3". Used as inline CSS.
    /// </summary>
    public string IconBgHex { get; set; } = string.Empty;

    /// <summary>
    /// Hex color for icon foreground, e.g. "#CA8A04". Used as inline CSS.
    /// </summary>
    public string IconFgHex { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the modal to open (if this opens a modal).
    /// </summary>
    public string? ModalTarget { get; set; }

    /// <summary>
    /// The URL to navigate to (fallback for no-JS or non-modal).
    /// </summary>
    public string NavigateUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether this link opens a modal.
    /// </summary>
    public bool IsModal => !string.IsNullOrEmpty(ModalTarget);

    /// <summary>
    /// Whether clicking triggers the inline health check panel instead of navigating.
    /// </summary>
    public bool IsHealthCheck { get; set; }
}

/// <summary>
/// ViewModel for the Engagement Widget (circular gauge + labeled progress bars).
/// </summary>
public class EngagementWidgetViewModel
{
    /// <summary>
    /// The widget title.
    /// </summary>
    public string Title { get; set; } = "Weekly Engagement Rate";

    /// <summary>
    /// The percentage for the circular gauge (0-100).
    /// </summary>
    public int GaugePercentage { get; set; }

    /// <summary>
    /// The label for the gauge.
    /// </summary>
    public string GaugeLabel { get; set; } = "Full Completion Rate";

    /// <summary>
    /// The Tailwind color class for the gauge arc.
    /// </summary>
    public string GaugeColor { get; set; } = "text-yellow-400";

    /// <summary>
    /// The labeled progress bars below the gauge.
    /// </summary>
    public List<EngagementBarViewModel> Bars { get; set; } = new();

    /// <summary>
    /// Widget size for layout purposes.
    /// </summary>
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

/// <summary>
/// ViewModel for a single engagement progress bar.
/// </summary>
public class EngagementBarViewModel
{
    /// <summary>
    /// The label displayed on the left.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The sublabel displayed on the right.
    /// </summary>
    public string SubLabel { get; set; } = string.Empty;

    /// <summary>
    /// The current value (numerator).
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// The maximum value (denominator).
    /// </summary>
    public int Max { get; set; } = 100;

    /// <summary>
    /// The display text shown inside the progress bar.
    /// </summary>
    public string DisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// The Tailwind color class for the progress bar fill.
    /// Prefer ColorClass (computed from Percentage) over this field.
    /// </summary>
    public string Color { get; set; } = "bg-green-500";

    /// <summary>
    /// Computed percentage for the progress bar width.
    /// </summary>
    public int Percentage => Max > 0 ? (int)Math.Round((double)Current / Max * 100) : 0;

    /// <summary>
    /// Computed Tailwind color class based on Percentage using the 6-band scale.
    /// </summary>
    public string ColorClass => GetBarColor(Percentage);

    public static string GetBarColor(int percentage) => percentage switch
    {
        < 17 => "bg-red-500",
        < 34 => "bg-red-400",
        < 50 => "bg-orange-500",
        < 67 => "bg-orange-400",
        < 84 => "bg-green-500",
        _    => "bg-green-600"
    };
}

/// <summary>
/// ViewModel for the Chart Widget.
/// </summary>
public class ChartWidgetViewModel
{
    /// <summary>
    /// The title displayed above the chart.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The canvas ID for Chart.js targeting.
    /// </summary>
    public string CanvasId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The chart type ('bar', 'line', 'bubble', etc.).
    /// </summary>
    public string Type { get; set; } = "line";

    /// <summary>
    /// The chart data object (serialized to JSON).
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Optional chart options.
    /// </summary>
    public object? Options { get; set; }

    /// <summary>
    /// Available period filters (e.g., 7d, 1m, total).
    /// </summary>
    public List<PeriodViewModel> Periods { get; set; } = new();

    /// <summary>
    /// Currently active period.
    /// </summary>
    public string ActivePeriod { get; set; } = "7d";

    /// <summary>
    /// Pre-rendered datasets keyed by period id for client-side tab switching.
    /// </summary>
    public Dictionary<string, object> PeriodDatasets { get; set; } = new();

    /// <summary>
    /// Widget size for layout purposes.
    /// </summary>
    public WidgetSize Size { get; set; } = WidgetSize.Medium;
}

/// <summary>
/// ViewModel for a period filter option.
/// </summary>
public class PeriodViewModel
{
    /// <summary>
    /// The period identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display label.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Whether this period is currently active.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// ViewModel for the Stat Widget.
/// </summary>
public class StatWidgetViewModel
{
    /// <summary>
    /// The label displayed at the top.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The main value displayed prominently.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Optional sublabel displayed below the value.
    /// </summary>
    public string? SubLabel { get; set; }

    /// <summary>
    /// Optional icon.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Optional color theme.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Widget size for layout purposes.
    /// </summary>
    public WidgetSize Size { get; set; } = WidgetSize.Small;
}

/// <summary>
/// Enum for widget size categories.
/// Height is content-determined, not fixed. M/L/XL span 3 grid rows = 3x Small widget height.
/// </summary>
public enum WidgetSize
{
    /// <summary>
    /// Small widget - cannot resize.
    /// </summary>
    Small,
    /// <summary>
    /// Medium widget - can resize to Large.
    /// </summary>
    Medium,
    /// <summary>
    /// Large widget - can resize to Medium or ExtraLarge.
    /// </summary>
    Large,
    /// <summary>
    /// Extra Large widget - can resize to Large.
    /// </summary>
    ExtraLarge
}
