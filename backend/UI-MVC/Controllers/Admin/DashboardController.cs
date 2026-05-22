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
    public IActionResult Index()
    {
        // Redirect to role-specific dashboard
        if (_adminContext.CurrentAdmin.IsConverseyAdmin())
        {
            return RedirectToAction(nameof(ConverseyDashboard));
        }
        else if (_adminContext.CurrentAdmin.IsWorkspaceAdmin())
        {
            return RedirectToAction(nameof(WorkspaceDashboard));
        }

        // If no recognized admin type, forbid access
        return Forbid();
    }

    /// <summary>
    /// Dashboard for Conversey (Platform) Administrators.
    /// Shows platform-wide statistics and management options.
    /// </summary>
    [HttpGet("/admin/dashboard/conversey")]
    [Authorize(Policy = ConverseyAdminPolicy.Name)]
    public async Task<IActionResult> ConverseyDashboard()
    {
        var dashboardData = await _adminStatsService.GetPlatformDashboardAsync();
        var viewModel = MapToViewModel(dashboardData);
        viewModel.AdminType = "conversey";
        viewModel.PageTitle = "Dashboard";
        viewModel.PageDescription = "Look at statistics, expand/manage our community.";

        // Add Conversey Admin specific nav cards
        viewModel.NavCards = new List<NavCardViewModel>
        {
            new() { Title = "Manage Workspaces", Description = "Create and manage all workspaces on the platform", Icon = "🏢", NavigateUrl = "/admin/workspaces", IconBackground = "bg-purple-100" },
            new() { Title = "AI Configuration", Description = "Configure AI providers and settings", Icon = "🤖", NavigateUrl = "/admin/ai", IconBackground = "bg-blue-100" }
        };

        return View(viewModel);
    }

    /// <summary>
    /// Dashboard for Workspace Administrators.
    /// Shows workspace-specific statistics and management options.
    /// </summary>
    [HttpGet("/admin/dashboard/workspace")]
    [Authorize(Policy = WorkspaceAdminPolicy.Name)]
    public async Task<IActionResult> WorkspaceDashboard()
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
            new() { Title = "Manage Projects", Description = "Create and manage projects in this workspace", Icon = "📁", NavigateUrl = "/admin/projects", IconBackground = "bg-orange-100" },
            new() { Title = "AI Settings", Description = "Configure AI for this workspace", Icon = "🤖", NavigateUrl = $"/admin/{workspaceId}/ai", IconBackground = "bg-green-100" }
        };

        return View(viewModel);
    }

    private static DashboardViewModel MapToViewModel(DashboardStatsDto dto) =>
        DashboardViewModelExtensions.FromDto(dto);

}
