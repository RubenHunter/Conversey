using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/ai")]
[Authorize(Policy = ConverseyAdminPolicy.Name)]
public class AiController : ControllerBase
{
    private readonly IAiAdminManager _aiAdminManager;

    public AiController(IAiAdminManager aiAdminManager)
    {
        _aiAdminManager = aiAdminManager;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AiHealthInfo), 200)]
    [ProducesResponseType(typeof(AiHealthInfo), 503)]
    public async Task<ActionResult> GetHealth()
    {
        var health = await _aiAdminManager.GetHealthAsync();
        var allOk = health.Status == "ok";
        return allOk ? Ok(health) : StatusCode(503, health);
    }

    [HttpGet("costs")]
    [ProducesResponseType(typeof(IEnumerable<AiAuditLog>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetAiCosts()
    {
        try
        {
            var costs = await _aiAdminManager.GetAllCostsAsync();
            return Ok(costs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve AI cost data", details = ex.Message });
        }
    }

    [HttpGet("costs/summary")]
    [ProducesResponseType(typeof(AiCostsSummary), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetAiCostsSummary()
    {
        try
        {
            var summary = await _aiAdminManager.GetCostsSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to generate cost summary", details = ex.Message });
        }
    }

    [HttpGet("costs/recent")]
    [ProducesResponseType(typeof(IEnumerable<AiAuditLog>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetRecentAiCosts([FromQuery] int days = 30)
    {
        if (days <= 0 || days > 365)
        {
            return BadRequest("Days parameter must be between 1 and 365");
        }

        try
        {
            var recentCosts = await _aiAdminManager.GetRecentCostsAsync(days);
            return Ok(recentCosts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve recent AI cost data", details = ex.Message });
        }
    }

    [HttpGet("providers/{id:int}/models")]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> ListProviderModels(int id)
    {
        try
        {
            var models = await _aiAdminManager.ListProviderModelsAsync(id);
            return Ok(models);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to list provider models", details = ex.Message });
        }
    }
}
