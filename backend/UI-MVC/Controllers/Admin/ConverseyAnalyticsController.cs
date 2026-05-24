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
    public IActionResult Index()
    {
        List<PlatformWorkspaceStatDto> platformStats;
        try
        {
            platformStats = _analyticsManager.GetPlatformStats();
        }
        catch
        {
            platformStats = new List<PlatformWorkspaceStatDto>();
        }

        var vm = new ConverseyAnalyticsViewModel
        {
            PlatformStats = platformStats,
            Filter = new AnalyticsFilterViewModel(),
            PlatformStatsJson = JsonSerializer.Serialize(platformStats, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            DashboardJson = "{}"
        };

        return View("~/Views/ConverseyAdmin/Analytics/Index.cshtml", vm);
    }

    [HttpGet("/admin/conversey/analytics/workspace/{slug}")]
    public async Task<IActionResult> WorkspaceDetail(string slug)
    {
        var platformStats = _analyticsManager.GetPlatformStats();
        var workspaceSlug = Slug.FromName(slug);

        AnalyticsDashboardDto dashboard;
        try
        {
            dashboard = await _analyticsManager.GetDashboardAsync(workspaceSlug, null, null);
        }
        catch
        {
            dashboard = new AnalyticsDashboardDto();
        }

        var vm = new ConverseyAnalyticsViewModel
        {
            PlatformStats = platformStats,
            Filter = new AnalyticsFilterViewModel(),
            SelectedWorkspaceDashboard = dashboard,
            PlatformStatsJson = JsonSerializer.Serialize(platformStats, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            DashboardJson = JsonSerializer.Serialize(dashboard, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        };

        return View("~/Views/ConverseyAdmin/Analytics/Index.cshtml", vm);
    }
}
