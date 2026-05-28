using Conversey.BL.Administration;
using Conversey.BL.Analytics;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Controllers.Admin.Helpers;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Models.Admin;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize]
public class DashboardController : Controller
{
    private readonly AdminContext _adminContext;
    private readonly IAnalyticsManager _analyticsManager;
    private readonly IProjectManager _projectManager;

    public DashboardController(AdminContext adminContext, IAnalyticsManager analyticsManager, IProjectManager projectManager)
    {
        _adminContext = adminContext;
        _analyticsManager = analyticsManager;
        _projectManager = projectManager;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
        try
        {
            if (_adminContext.CurrentAdmin.IsConverseyAdmin())
            {
                return await BuildConverseyAdminDashboard();
            }
            else if (_adminContext.CurrentAdmin.IsWorkspaceAdmin())
            {
                return await BuildWorkspaceAdminDashboard();
            }

            return Forbid();
        }
        catch (Exception)
        {
            var errorVm = new DashboardViewModel
            {
                AdminType = "error",
                PageTitle = "Dashboard",
                PageDescription = "An error occurred while loading the dashboard. Please try again later.",
                NavCards = new List<NavCardViewModel>(),
                StatWidgets = new List<StatWidgetViewModel>(),
                ComparisonWidget = null,
                QuickLinksWidget = null,
                EngagementWidget = null,
                UsageTrendChart = null
            };

            return View("~/Views/Admin/Dashboard.cshtml", errorVm);
        }
    }

    private async Task<IActionResult> BuildConverseyAdminDashboard()
    {
        var platformStats = _analyticsManager.GetPlatformStats();
        var usageTrend = _analyticsManager.GetUsageTrend();
        var moderation = _analyticsManager.GetPlatformModerationStats();

        var viewModel = new DashboardViewModel
        {
            AdminType = "conversey",
            PageTitle = "Dashboard",
            PageDescription = "Look at statistics, expand/manage our community.",
            NavCards = new List<NavCardViewModel>
            {
                new() { Title = "Manage Workspaces", Description = "Create and manage all workspaces on the platform", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2'/></svg>", NavigateUrl = "/admin/workspaces", IconBackground = "bg-purple-100", IconColor = "text-purple-600" },
                new() { Title = "Costs", Description = "Track AI usage costs across the platform", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M9 5h6m2 0h2v14H3V5h2m4 0v14'/></svg>", NavigateUrl = "/admin/ai/costs", IconBackground = "bg-amber-100", IconColor = "text-amber-600" },
                new() { Title = "AI Configuration", Description = "Configure AI providers and settings", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.324.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.24-.438.613-.431.992a6.759 6.759 0 010 .255c-.007.378.138.75.43.99l1.005.828c.424.35.534.954.26 1.43l-1.298 2.247a1.125 1.125 0 01-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.57 6.57 0 01-.22.128c-.331.183-.581.495-.644.869l-.213 1.28c-.09.543-.56.941-1.11.941h-2.594c-.55 0-1.02-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.992a6.932 6.932 0 010-.255c.007-.378-.138-.75-.43-.99l-1.004-.828a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.087.22-.128.332-.183.582-.495.644-.869l.214-1.281z'/><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M15 12a3 3 0 11-6 0 3 3 0 016 0z'/></svg>", NavigateUrl = "/admin/ai", IconBackground = "bg-blue-100", IconColor = "text-blue-600" }
            },
            StatWidgets = new List<StatWidgetViewModel>
            {
                new() { Label = "Workspaces", Value = platformStats.Count.ToString(), SubLabel = "Total active workspaces" },
                new() { Label = "Projects", Value = platformStats.Sum(p => p.ProjectCount).ToString(), SubLabel = "Across all workspaces" },
                new() { Label = "Youths", Value = platformStats.Sum(p => p.YouthCount).ToString(), SubLabel = "Total participants" },
                new() { Label = "Ideas", Value = platformStats.Sum(p => p.IdeaCount).ToString(), SubLabel = "Total submissions" }
            },
            ComparisonWidget = DashboardWidgetBuilders.BuildPlatformComparisonWidget(platformStats),
            QuickLinksWidget = new QuickLinksWidgetViewModel
            {
                Title = "Links",
                Items = new List<QuickLinkItemViewModel>
                {
                    new() { Title = "New workspace", Description = "Create a new workspace", Icon = "<svg class='w-5 h-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M12 4.5v15m7.5-7.5h-15'/></svg>", IconBgHex = "#FEF9C3", IconFgHex = "#CA8A04", ModalTarget = "modal-new-workspace", NavigateUrl = "/admin/workspaces/new" },
                    new() { Title = "Check health", Description = "Check AI provider", Icon = "<svg class='w-5 h-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12Z'/></svg>", IconBgHex = "#FFE4EC", IconFgHex = "#FF2F6A", IsHealthCheck = true, NavigateUrl = "" },
                    new() { Title = "Rate Limits", Description = "Limits for AI endpoints", Icon = "<svg class='w-5 h-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M10.5 6h9.75M10.5 6a1.5 1.5 0 11-3 0m3 0a1.5 1.5 0 10-3 0M3.75 6H7.5m3 12h9.75m-9.75 0a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m-3.75 0H7.5m9-6h3.75m-3.75 0a1.5 1.5 0 01-3 0m3 0a1.5 1.5 0 00-3 0m-9.75 0h9.75'/></svg>", IconBgHex = "#ECFCE7", IconFgHex = "#6DE92A", NavigateUrl = "/admin/ai/rate-limits" }
                }
            },
            EngagementWidget = DashboardWidgetBuilders.BuildPlatformEngagementWidget(platformStats, moderation),
            UsageTrendChart = DashboardWidgetBuilders.BuildUsageTrendChart(usageTrend),
            UsageTrendJson = DashboardWidgetBuilders.SerializeUsageTrendJson(usageTrend)
        };

        return View("~/Views/Admin/Dashboard.cshtml", viewModel);
    }

    private async Task<IActionResult> BuildWorkspaceAdminDashboard()
    {
        var workspace = _adminContext.GetWorkspace();
        var workspaceName = workspace?.Name ?? "Workspace";
        var workspaceId = workspace?.Id ?? Slug.FromName("unknown");

        var dashboard = await _analyticsManager.GetDashboardAsync(workspaceId, null, null);
        var usageTrend = _analyticsManager.GetUsageTrend(workspaceId);
        var moderation = _analyticsManager.GetPlatformModerationStats(workspaceId);

        var viewModel = new DashboardViewModel
        {
            AdminType = "workspace",
            WorkspaceName = workspaceName,
            WorkspaceId = workspaceId.ToString(),
            PageTitle = "Dashboard",
            PageDescription = "Manage your workspace.",
            NavCards = new List<NavCardViewModel>
            {
                new() { Title = "Manage Projects", Description = "Create and manage projects in this workspace", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25H12'/></svg>", NavigateUrl = "/admin/projects", IconBackground = "bg-orange-100", IconColor = "text-orange-600" },
                new() { Title = "Costs", Description = "Track AI usage costs for this workspace", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M9 5h6m2 0h2v14H3V5h2m4 0v14'/></svg>", NavigateUrl = $"/admin/{workspaceId}/ai/costs", IconBackground = "bg-amber-100", IconColor = "text-amber-600" },
                new() { Title = "AI Settings", Description = "Configure AI for this workspace", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M9.594 3.94c.09-.542.56-.94 1.11-.94h2.593c.55 0 1.02.398 1.11.94l.213 1.281c.063.374.313.686.645.87.074.04.147.083.22.127.324.196.72.257 1.075.124l1.217-.456a1.125 1.125 0 011.37.49l1.296 2.247a1.125 1.125 0 01-.26 1.431l-1.003.827c-.293.24-.438.613-.431.992a6.759 6.759 0 010 .255c-.007.378.138.75.43.99l1.005.828c.424.35.534.954.26 1.43l-1.298 2.247a1.125 1.125 0 01-1.369.491l-1.217-.456c-.355-.133-.75-.072-1.076.124a6.57 6.57 0 01-.22.128c-.331.183-.581.495-.644.869l-.213 1.28c-.09.543-.56.941-1.11.941h-2.594c-.55 0-1.02-.398-1.11-.94l-.213-1.281c-.062-.374-.312-.686-.644-.87a6.52 6.52 0 01-.22-.127c-.325-.196-.72-.257-1.076-.124l-1.217.456a1.125 1.125 0 01-1.369-.49l-1.297-2.247a1.125 1.125 0 01.26-1.431l1.004-.827c.292-.24.437-.613.43-.992a6.932 6.932 0 010-.255c.007-.378-.138-.75-.43-.99l-1.004-.828a1.125 1.125 0 01-.26-1.43l1.297-2.247a1.125 1.125 0 011.37-.491l1.216.456c.356.133.751.072 1.076-.124.072-.044.146-.087.22-.128.332-.183.582-.495.644-.869l.214-1.281z'/><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M15 12a3 3 0 11-6 0 3 3 0 016 0z'/></svg>", NavigateUrl = $"/admin/{workspaceId}/ai", IconBackground = "bg-green-100", IconColor = "text-green-600" }
            },
            StatWidgets = new List<StatWidgetViewModel>
            {
                new() { Label = "Youths", Value = dashboard.Participation.TotalYouth.ToString(), SubLabel = "Total participants" },
                new() { Label = "Ideas", Value = dashboard.Ideas.Count.ToString(), SubLabel = "Total submissions" },
                new() { Label = "Answers", Value = dashboard.OpenAnswers.Count.ToString(), SubLabel = "Survey responses" },
                new() { Label = "Conversion", Value = $"{dashboard.Participation.ConversionRate:F1}%", SubLabel = "Youth with ideas/answers" }
            },
            ComparisonWidget = DashboardWidgetBuilders.BuildWorkspaceComparisonWidget(_projectManager.GetAllProjectsFromWorkspaceId(workspaceId).ToList()),
            QuickLinksWidget = new QuickLinksWidgetViewModel
            {
                Title = "Links",
                Items = new List<QuickLinkItemViewModel>
                {
                    new() { Title = "New project", Description = "Create a new project", Icon = "<svg class='w-5 h-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M12 4.5v15m7.5-7.5h-15'/></svg>", IconBgHex = "#FEF9C3", IconFgHex = "#CA8A04", ModalTarget = "modal-new-project", NavigateUrl = "/admin/projects/new" },
                    new() { Title = "Check health", Description = "Check AI provider", Icon = "<svg class='w-5 h-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12Z'/></svg>", IconBgHex = "#FFE4EC", IconFgHex = "#FF2F6A", IsHealthCheck = true, NavigateUrl = "" },
                    new() { Title = "Analytics", Description = "View workspace statistics", Icon = "<svg class='w-5 h-5' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z'/></svg>", IconBgHex = "#ECFCE7", IconFgHex = "#22c55e", NavigateUrl = "/admin/workspace/analytics" }
                }
            },
            EngagementWidget = DashboardWidgetBuilders.BuildWorkspaceEngagementWidget(dashboard, moderation),
            UsageTrendChart = DashboardWidgetBuilders.BuildUsageTrendChart(usageTrend),
            UsageTrendJson = DashboardWidgetBuilders.SerializeUsageTrendJson(usageTrend)
        };

        return View("~/Views/Admin/Dashboard.cshtml", viewModel);
    }
}
