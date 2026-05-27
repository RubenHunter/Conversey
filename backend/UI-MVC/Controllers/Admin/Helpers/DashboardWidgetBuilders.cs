using System.Text.Json;
using Conversey.BL.Analytics.DTOs;
using Conversey.BL.Domain.Administration;
using Conversey.UI_MVC.Models.Admin;

namespace Conversey.UI_MVC.Controllers.Admin.Helpers;

public static class DashboardWidgetBuilders
{
    private static readonly string[] ComparisonColors =
        { "primary", "secondary", "accent", "indigo", "violet", "amber", "emerald", "rose", "cyan", "slate" };

    public static readonly PeriodViewModel[] UsageTrendPeriods =
    {
        new() { Id = "7d", Label = "7d", IsActive = false },
        new() { Id = "1m", Label = "1m", IsActive = true },
        new() { Id = "total", Label = "Total", IsActive = false }
    };

    public static ComparisonWidgetViewModel BuildPlatformComparisonWidget(List<PlatformWorkspaceStatDto> stats)
    {
        return new ComparisonWidgetViewModel
        {
            Title = "Workspaces",
            SubTitle = "All workspaces",
            TitleUrl = "/admin/conversey/analytics",
            SubTitleUrl = "/admin/workspaces",
            ToggleLabel = "Projects",
            ToggleSecondaryLabel = "Youths",
            Items = stats.Select((w, i) => new ComparisonItemViewModel
            {
                Label = w.WorkspaceName,
                Value = w.ProjectCount,
                Color = ComparisonColors[i % ComparisonColors.Length],
                NavigateUrl = $"/admin/conversey/analytics?workspaceId={w.WorkspaceSlug}"
            }).ToList(),
            AllItems = stats.Select((w, i) => new ComparisonItemViewModel
            {
                Label = w.WorkspaceName,
                Value = w.YouthCount,
                Color = ComparisonColors[i % ComparisonColors.Length],
                NavigateUrl = $"/admin/conversey/analytics?workspaceId={w.WorkspaceSlug}"
            }).ToList()
        };
    }

    public static ComparisonWidgetViewModel BuildWorkspaceComparisonWidget(List<Project> projects)
    {
        var projectItems = projects.Select(p => new ComparisonItemViewModel
        {
            Label = p.Name,
            Value = p.Youth?.Count() ?? 0,
            Color = "primary",
            NavigateUrl = $"/admin/workspace/analytics?projectId={p.Name}"
        }).ToList();

        return new ComparisonWidgetViewModel
        {
            Title = "Projects Overview",
            SubTitle = "All Projects",
            TitleUrl = "/admin/projects",
            SubTitleUrl = "/admin/projects",
            Items = projectItems,
            AllItems = projects.Where(p => p.Status == Status.Active).Select(p => new ComparisonItemViewModel
            {
                Label = p.Name,
                Value = p.Youth?.Count() ?? 0,
                Color = "secondary",
                NavigateUrl = $"/admin/workspace/analytics?projectId={p.Name}"
            }).ToList(),
            Size = WidgetSize.Large
        };
    }

    public static EngagementWidgetViewModel BuildPlatformEngagementWidget(List<PlatformWorkspaceStatDto> stats, PlatformModerationStatsDto moderation)
    {
        var totalYouth = stats.Sum(s => s.YouthCount);
        var totalIdeas = stats.Sum(s => s.IdeaCount);
        var avgIdeasPerYouth = totalYouth > 0 ? totalIdeas / (double)totalYouth : 0;
        var gaugePct = totalYouth > 0 ? Math.Min(100, (int)Math.Round((double)totalIdeas / totalYouth * 100)) : 0;

        return new EngagementWidgetViewModel
        {
            Title = "Engagement Overview",
            GaugePercentage = gaugePct,
            GaugeLabel = gaugePct >= 100 ? "Max Ideas/Youth" : "Idea Rate",
            GaugeColor = gaugePct >= 100 ? "text-green-400" : "text-yellow-400",
            Bars = new List<EngagementBarViewModel>
            {
                new() { Label = "Avg Ideas/Youth", SubLabel = $"{avgIdeasPerYouth:F1} per participant", Current = (int)Math.Round(avgIdeasPerYouth * 100), Max = 100, DisplayValue = $"{avgIdeasPerYouth:F1}", Color = "bg-green-500" },
                new() { Label = "Surveys", SubLabel = $"{totalYouth} youth participated", Current = totalYouth, Max = Math.Max(totalYouth, 1), DisplayValue = $"{totalYouth}", Color = "bg-blue-500" },
                new() { Label = "Flagged", SubLabel = $"{moderation.TotalFlaggedIdeas + moderation.TotalFlaggedComments} content items", Current = moderation.TotalFlaggedIdeas + moderation.TotalFlaggedComments, Max = Math.Max(totalIdeas, 1), DisplayValue = $"{(moderation.TotalFlaggedIdeas + moderation.TotalFlaggedComments)}", Color = "bg-orange-500" }
            }
        };
    }

    public static EngagementWidgetViewModel BuildWorkspaceEngagementWidget(AnalyticsDashboardDto dashboard, PlatformModerationStatsDto moderation)
    {
        var p = dashboard.Participation;
        return new EngagementWidgetViewModel
        {
            Title = "Participant Engagement",
            GaugePercentage = (int)Math.Round(p.ConversionRate),
            GaugeLabel = "Conversion Rate",
            GaugeColor = "text-green-400",
            Bars = new List<EngagementBarViewModel>
            {
                new() { Label = "Youth with Ideas", SubLabel = $"{p.YouthWithIdeas} of {p.TotalYouth}", Current = p.YouthWithIdeas, Max = Math.Max(p.TotalYouth, 1), DisplayValue = $"{p.YouthWithIdeas}/{p.TotalYouth}", Color = "bg-green-500" },
                new() { Label = "Avg Ideas/Youth", SubLabel = "Average per participant", Current = (int)Math.Round(p.AvgIdeasPerYouth * 100), Max = 100, DisplayValue = $"{p.AvgIdeasPerYouth:F1}", Color = "bg-purple-500" },
                new() { Label = "Flagged Content", SubLabel = "Moderation flags", Current = moderation.TotalFlaggedIdeas + moderation.TotalFlaggedComments, Max = Math.Max(dashboard.Ideas.Count, 1), DisplayValue = $"{moderation.TotalFlaggedIdeas + moderation.TotalFlaggedComments}", Color = "bg-orange-500" }
            }
        };
    }

    public static ChartWidgetViewModel BuildUsageTrendChart(List<UsageTrendPointDto> trend)
    {
        var labels = trend.Select(t => t.Date).ToList();
        var ideaValues = trend.Select(t => t.IdeaCount).Cast<object>().ToList();
        var youthValues = trend.Select(t => t.UniqueYouth).Cast<object>().ToList();

        var allValues = trend.SelectMany(t => new[] { t.IdeaCount, t.UniqueYouth }).DefaultIfEmpty(0);
        var yMax = allValues.Max() + 1;

        return new ChartWidgetViewModel
        {
            Title = "Usage Trend",
            Type = "line",
            Data = new
            {
                labels,
                datasets = new[]
                {
                    new { label = "Ideas", data = ideaValues, borderColor = "#6366f1", backgroundColor = "rgba(99,102,241,0.1)", tension = 0.3, fill = true },
                    new { label = "Active Youth", data = youthValues, borderColor = "#f97316", backgroundColor = "rgba(249,115,22,0.1)", tension = 0.3, fill = true }
                }
            },
            Options = new
            {
                scales = new
                {
                    y = new
                    {
                        beginAtZero = true,
                        max = yMax,
                        ticks = new { stepSize = 1 }
                    }
                },
                plugins = new
                {
                    legend = new { position = "bottom" }
                }
            },
            Periods = UsageTrendPeriods.ToList(),
            ActivePeriod = "1m"
        };
    }

    public static string SerializeUsageTrendJson(List<UsageTrendPointDto> trend)
    {
        return JsonSerializer.Serialize(trend.Select(t => new { date = t.Date, ideaCount = t.IdeaCount, uniqueYouth = t.UniqueYouth }));
    }
}
