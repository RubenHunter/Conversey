using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Models.Admin;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

/// <summary>
/// Unified dashboard controller that adapts to admin role.
/// Handles the /admin route and provides role-specific dashboard views.
/// </summary>
[Authorize]
public class DashboardController : Controller
{
    private readonly AdminContext _adminContext;
    private readonly WorkspaceContext _workspaceContext;
    private readonly IAdminStatsService _adminStatsService;

    public DashboardController(AdminContext adminContext, WorkspaceContext workspaceContext, IAdminStatsService adminStatsService)
    {
        _adminContext = adminContext;
        _workspaceContext = workspaceContext;
        _adminStatsService = adminStatsService;
    }

    /// <summary>
    /// Main dashboard endpoint that adapts based on admin role.
    /// Conversey Admins see platform-wide dashboard.
    /// Workspace Admins see workspace-specific dashboard.
    /// </summary>
    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
    {
        if (_adminContext.CurrentAdmin.IsConverseyAdmin())
        {
            var dashboardData = await _adminStatsService.GetPlatformDashboardAsync();
            var viewModel = MapToViewModel(dashboardData);
            viewModel.AdminType = "conversey";
            viewModel.PageTitle = "Dashboard";
            viewModel.PageDescription = "Look at statistics, expand/manage our community.";

            // Add Conversey Admin specific nav cards
            viewModel.NavCards = new List<NavCardViewModel>
            {
                new() { Title = "Manage Workspaces", Description = "Create and manage all workspaces on the platform", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2'/></svg>", NavigateUrl = "/admin/workspaces", IconBackground = "bg-purple-100", IconColor = "text-purple-600" },
                new() { Title = "AI Configuration", Description = "Configure AI providers and settings", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg>", NavigateUrl = "/admin/ai", IconBackground = "bg-blue-100", IconColor = "text-blue-600" }
            };

            return View("~/Views/Admin/Dashboard.cshtml", viewModel);
        }
        else if (_adminContext.CurrentAdmin.IsWorkspaceAdmin())
        {
            var workspace = _adminContext.GetWorkspace();
            var workspaceName = workspace?.Name ?? "Workspace";
            var workspaceId = workspace?.Id ?? Slug.FromName("unknown");

            var dashboardData = await _adminStatsService.GetWorkspaceDashboardAsync(workspaceId);
            var viewModel = MapToViewModel(dashboardData);
            viewModel.AdminType = "workspace";
            viewModel.WorkspaceName = workspaceName;
            viewModel.WorkspaceId = workspaceId.ToString();
            viewModel.PageTitle = "Dashboard";
            viewModel.PageDescription = "Manage your workspace.";

            // Add Workspace Admin specific nav cards
            viewModel.NavCards = new List<NavCardViewModel>
            {
                new() { Title = "Manage Projects", Description = "Create and manage projects in this workspace", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25H12'/></svg>", NavigateUrl = "/admin/projects", IconBackground = "bg-orange-100", IconColor = "text-orange-600" },
                new() { Title = "AI Settings", Description = "Configure AI for this workspace", Icon = "<svg class='w-6 h-6' fill='none' stroke='currentColor' viewBox='0 0 24 24'><path stroke-linecap='round' stroke-linejoin='round' stroke-width='1.5' d='M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'/></svg>", NavigateUrl = $"/admin/{workspaceId}/ai", IconBackground = "bg-green-100", IconColor = "text-green-600" }
            };

            return View("~/Views/Admin/Dashboard.cshtml", viewModel);
        }

        // If no recognized admin type, forbid access
        return Forbid();
    }

    private static DashboardViewModel MapToViewModel(DashboardStatsDto dto) =>
        DashboardViewModelExtensions.FromDto(dto);

}
