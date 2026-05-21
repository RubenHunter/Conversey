using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

/// <summary>
/// API controller for admin statistics and dashboard data.
/// Returns role-filtered data based on the current admin's permissions.
/// </summary>
[Authorize]
[Route("api/admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IAdminStatsService _adminStatsService;
    private readonly AdminContext _adminContext;

    public AdminStatsController(IAdminStatsService adminStatsService, AdminContext adminContext)
    {
        _adminStatsService = adminStatsService;
        _adminContext = adminContext;
    }

    /// <summary>
    /// Gets dashboard data for the current admin.
    /// Returns platform-wide data for Conversey Admins, workspace-specific data for Workspace Admins.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardData([FromQuery] string? period = null)
    {
        var currentAdmin = _adminContext.CurrentAdmin;
        
        if (currentAdmin == null)
        {
            return Unauthorized();
        }

        if (currentAdmin is ConverseyAdmin)
        {
            var dashboardData = await _adminStatsService.GetPlatformDashboardAsync();
            return Ok(new
            {
                type = "platform",
                data = dashboardData
            });
        }
        else if (currentAdmin is WorkspaceAdmin workspaceAdmin)
        {
            var dashboardData = await _adminStatsService.GetWorkspaceDashboardAsync(workspaceAdmin.Workspace.Id);
            return Ok(new
            {
                type = "workspace",
                data = dashboardData,
                workspaceName = workspaceAdmin.Workspace.Name,
                workspaceId = workspaceAdmin.Workspace.Id.ToString()
            });
        }

        return Forbid();
    }

    /// <summary>
    /// Gets platform-wide statistics (Conversey Admin only).
    /// </summary>
    [HttpGet("stats/platform")]
    [Authorize(Policy = ConverseyAdminPolicy.Name)]
    public async Task<IActionResult> GetPlatformStats()
    {
        var stats = await _adminStatsService.GetPlatformStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Gets workspace-specific statistics.
    /// Workspace Admins can only access their own workspace.
    /// Conversey Admins can access any workspace.
    /// </summary>
    [HttpGet("stats/workspace/{workspaceId}")]
    public async Task<IActionResult> GetWorkspaceStats(string workspaceId)
    {
        var currentAdmin = _adminContext.CurrentAdmin;
        
        if (currentAdmin == null)
        {
            return Unauthorized();
        }

        // Workspace Admins can only access their own workspace
        if (currentAdmin is WorkspaceAdmin workspaceAdmin)
        {
            if (workspaceAdmin.Workspace.Id.ToString() != workspaceId)
            {
                return Forbid();
            }
        }
        // Conversey Admins can access any workspace
        else if (currentAdmin is not ConverseyAdmin)
        {
            return Forbid();
        }

        var stats = await _adminStatsService.GetWorkspaceStatsAsync(Conversey.BL.Domain.Common.Slug.FromName(workspaceId));
        return Ok(stats);
    }

    /// <summary>
    /// Gets usage trend data for the current admin.
    /// </summary>
    [HttpGet("stats/usage-trend")]
    public async Task<IActionResult> GetUsageTrend([FromQuery] string period = "7d")
    {
        var currentAdmin = _adminContext.CurrentAdmin;
        
        if (currentAdmin == null)
        {
            return Unauthorized();
        }

        var trend = await _adminStatsService.GetUsageTrendAsync(currentAdmin, period);
        return Ok(trend);
    }

    /// <summary>
    /// Gets comparison widget data for the current admin.
    /// </summary>
    [HttpGet("stats/comparison")]
    public async Task<IActionResult> GetComparisonData()
    {
        var currentAdmin = _adminContext.CurrentAdmin;
        
        if (currentAdmin == null)
        {
            return Unauthorized();
        }

        var comparison = await _adminStatsService.GetComparisonDataAsync(currentAdmin);
        return Ok(comparison);
    }

    /// <summary>
    /// Gets quick links widget data for the current admin.
    /// </summary>
    [HttpGet("stats/quick-links")]
    public async Task<IActionResult> GetQuickLinksData()
    {
        var currentAdmin = _adminContext.CurrentAdmin;
        
        if (currentAdmin == null)
        {
            return Unauthorized();
        }

        var quickLinks = await _adminStatsService.GetQuickLinksDataAsync(currentAdmin);
        return Ok(quickLinks);
    }

    /// <summary>
    /// Gets engagement widget data for the current admin.
    /// </summary>
    [HttpGet("stats/engagement")]
    public async Task<IActionResult> GetEngagementData()
    {
        var currentAdmin = _adminContext.CurrentAdmin;
        
        if (currentAdmin == null)
        {
            return Unauthorized();
        }

        var engagement = await _adminStatsService.GetEngagementDataAsync(currentAdmin);
        return Ok(engagement);
    }
}
