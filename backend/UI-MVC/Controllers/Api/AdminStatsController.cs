using Conversey.BL.Ai;
using Conversey.BL.Analytics;
using Conversey.BL.Domain.Administration;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[Authorize]
[Route("api/admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IAnalyticsManager _analyticsManager;
    private readonly IAiAdminManager _aiAdminManager;
    private readonly AdminContext _adminContext;

    public AdminStatsController(IAnalyticsManager analyticsManager, IAiAdminManager aiAdminManager, AdminContext adminContext)
    {
        _analyticsManager = analyticsManager;
        _aiAdminManager = aiAdminManager;
        _adminContext = adminContext;
    }

    [HttpGet("stats/usage-trend")]
    public IActionResult GetUsageTrend([FromQuery] string period = "1m")
    {
        var admin = _adminContext.CurrentAdmin;

        if (admin is WorkspaceAdmin workspaceAdmin)
        {
            var trend = _analyticsManager.GetUsageTrend(workspaceAdmin.Workspace.Id, null);
            return Ok(new { labels = trend.Select(t => t.Date), values = trend.Select(t => t.IdeaCount) });
        }

        var platformTrend = _analyticsManager.GetUsageTrend(null, null);
        return Ok(new { labels = platformTrend.Select(t => t.Date), values = platformTrend.Select(t => t.IdeaCount) });
    }

    [HttpGet("ai/health")]
    public async Task<IActionResult> GetAiHealth()
    {
        try
        {
            var health = await _aiAdminManager.GetHealthAsync();
            return Ok(new[]
            {
                new { provider = "System", status = health.Status == "ok" ? "ok" : "error", latencyMs = 0 },
                new { provider = "Moderation", status = health.Moderation?.Ok == true ? "ok" : "error", latencyMs = health.Moderation?.DurationMs ?? 0 },
                new { provider = "Completions", status = health.Completions?.Ok == true ? "ok" : "error", latencyMs = health.Completions?.DurationMs ?? 0 }
            });
        }
        catch (Exception)
        {
            return Ok(new[]
            {
                new { provider = "System", status = "error", latencyMs = 0 },
                new { provider = "Moderation", status = "error", latencyMs = 0 },
                new { provider = "Completions", status = "error", latencyMs = 0 }
            });
        }
    }
}
