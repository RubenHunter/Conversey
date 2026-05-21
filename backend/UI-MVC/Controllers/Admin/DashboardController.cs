using Conversey.BL.Domain.Administration;
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

    public DashboardController(AdminContext adminContext, WorkspaceContext workspaceContext)
    {
        _adminContext = adminContext;
        _workspaceContext = workspaceContext;
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
    public IActionResult ConverseyDashboard()
    {
        return View("Dashboard", new DashboardViewModel
        {
            PageTitle = "Dashboard",
            PageDescription = "Look at statistics, expand/manage our community.",
            AdminType = "conversey"
        });
    }

    /// <summary>
    /// Dashboard for Workspace Administrators.
    /// Shows workspace-specific statistics and management options.
    /// </summary>
    [HttpGet("/admin/dashboard/workspace")]
    [Authorize(Policy = WorkspaceAdminPolicy.Name)]
    public IActionResult WorkspaceDashboard()
    {
        var workspace = _adminContext.GetWorkspace();
        var workspaceName = workspace?.Name ?? "Workspace";

        return View("Dashboard", new DashboardViewModel
        {
            PageTitle = "Dashboard",
            PageDescription = $"Manage {workspaceName}",
            AdminType = "workspace",
            WorkspaceName = workspaceName,
            WorkspaceId = workspace?.Id.ToString()
        });
    }

    /// <summary>
    /// Legacy redirect for existing links.
    /// </summary>
    [HttpGet("/admin/conversey")]
    [Authorize(Policy = ConverseyAdminPolicy.Name)]
    public IActionResult ConverseyRedirect()
    {
        return RedirectToAction(nameof(ConverseyDashboard));
    }

    /// <summary>
    /// Legacy redirect for existing links.
    /// </summary>
    [HttpGet("/admin/workspace")]
    [Authorize(Policy = WorkspaceAdminPolicy.Name)]
    public IActionResult WorkspaceRedirect()
    {
        return RedirectToAction(nameof(WorkspaceDashboard));
    }
}
