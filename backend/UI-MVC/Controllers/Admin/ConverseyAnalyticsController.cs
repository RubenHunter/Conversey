using System.Globalization;
using System.Text.Json;
using Conversey.BL.Administration;
using Conversey.BL.Analytics;
using Conversey.BL.Analytics.DTOs;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models.Analytics;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize(Policy = ConverseyAdminPolicy.Name)]
public class ConverseyAnalyticsController : Controller
{
    private readonly IAnalyticsManager _analyticsManager;

    public ConverseyAnalyticsController(IAnalyticsManager analyticsManager)
    {
        _analyticsManager = analyticsManager;
    }

    [HttpGet("/admin/conversey/analytics")]
    public IActionResult Index(
        [FromQuery] string? workspaceId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        Slug? wsSlug = string.IsNullOrWhiteSpace(workspaceId) ? null : Slug.FromName(workspaceId);
        DateTime? parsedDateFrom = ParseDate(Request.Query["dateFrom"]);
        DateTime? parsedDateTo = ParseDate(Request.Query["dateTo"]);

        List<PlatformWorkspaceStatDto> platformStats;
        PlatformModerationStatsDto? modStats = null;
        PlatformUserStatsDto? userStats = null;
        List<UsageTrendPointDto> trend = new();

        try
        {
            platformStats = _analyticsManager.GetPlatformStats(wsSlug);
        }
        catch
        {
            platformStats = new List<PlatformWorkspaceStatDto>();
        }

        try
        {
            modStats = _analyticsManager.GetPlatformModerationStats(wsSlug);
        }
        catch { }

        try
        {
            userStats = _analyticsManager.GetPlatformUserStats(wsSlug);
        }
        catch { }

        try
        {
            trend = _analyticsManager.GetUsageTrend(wsSlug, null, parsedDateFrom, parsedDateTo);
        }
        catch { }

        var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var allPlatformStats = new List<PlatformWorkspaceStatDto>();
        try { allPlatformStats = _analyticsManager.GetPlatformStats(); } catch { }

        var circlesData = allPlatformStats.Select(ws => new
        {
            name = ws.WorkspaceName,
            slug = ws.WorkspaceSlug,
            youthCount = ws.YouthCount,
            projectCount = ws.ProjectCount,
            ideaCount = ws.IdeaCount
        }).ToList();

        var vm = new ConverseyAnalyticsViewModel
        {
            PlatformStats = platformStats,
            Filter = new AnalyticsFilterViewModel { DateFrom = parsedDateFrom, DateTo = parsedDateTo },
            ModerationStats = modStats,
            UserStats = userStats,
            AllPlatformStats = allPlatformStats,
            SelectedWorkspaceId = workspaceId,
            PlatformStatsJson = JsonSerializer.Serialize(platformStats, jsonOpts),
            ModerationStatsJson = modStats != null ? JsonSerializer.Serialize(modStats, jsonOpts) : "{}",
            UserStatsJson = userStats != null ? JsonSerializer.Serialize(userStats, jsonOpts) : "{}",
            UsageTrendJson = JsonSerializer.Serialize(trend, jsonOpts),
            DashboardJson = "{}",
            WorkspaceCirclesJson = JsonSerializer.Serialize(circlesData, jsonOpts)
        };

        return View("~/Views/ConverseyAdmin/Analytics/Index.cshtml", vm);
    }

    [HttpGet("/admin/conversey/analytics/workspace/{slug}")]
    public async Task<IActionResult> WorkspaceDetail(string slug)
    {
        var allPlatformStats = _analyticsManager.GetPlatformStats();
        var platformStats = _analyticsManager.GetPlatformStats(Slug.FromName(slug));
        var workspaceSlug = Slug.FromName(slug);

        AnalyticsDashboardDto dashboard;
        List<UsageTrendPointDto> trend = new();
        try
        {
            dashboard = await _analyticsManager.GetDashboardAsync(workspaceSlug, null, null);
        }
        catch
        {
            dashboard = new AnalyticsDashboardDto();
        }

        try
        {
            trend = _analyticsManager.GetUsageTrend(workspaceSlug);
        }
        catch { }

        var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var vm = new ConverseyAnalyticsViewModel
        {
            PlatformStats = platformStats,
            AllPlatformStats = allPlatformStats,
            Filter = new AnalyticsFilterViewModel(),
            SelectedWorkspaceDashboard = dashboard,
            SelectedWorkspaceId = slug,
            PlatformStatsJson = JsonSerializer.Serialize(platformStats, jsonOpts),
            DashboardJson = JsonSerializer.Serialize(dashboard, jsonOpts),
            UsageTrendJson = JsonSerializer.Serialize(trend, jsonOpts)
        };

        return View("~/Views/ConverseyAdmin/Analytics/Index.cshtml", vm);
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;
        return null;
    }
}
