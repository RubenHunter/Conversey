using Conversey.BL.Domain;
using Conversey.BL.Ai;
using Conversey.DAL.Subplatform.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/ai")]
[Authorize(Roles = "Admin")]
public class AiController : Controller
{
    private readonly IAuditRepository _auditRepository;
    private readonly IAiManager _aiManager;
    private readonly IConfiguration _configuration;

    public AiController(IAuditRepository auditRepository, IAiManager aiManager, IConfiguration configuration)
    {
        _auditRepository = auditRepository;
        _aiManager = aiManager;
        _configuration = configuration;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(503)]
    public async Task<ActionResult> GetHealth()
    {
        var provider = (_configuration["AI:Provider"] ?? "Unknown").Trim();
        var managerType = _aiManager.GetType().Name;
        var apiKeyConfigured = !string.IsNullOrWhiteSpace(_configuration["AI:Mistral:ApiKey"]);

        try
        {
            var moderationDecision = await _aiManager.ModerateContent("health-check: keep this sentence respectful");

            return Ok(new
            {
                status = "ok",
                provider,
                managerType,
                apiKeyConfigured,
                moderationProbe = new
                {
                    isAllowed = moderationDecision.IsAllowed
                },
                checkedAtUtc = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                status = "error",
                provider,
                managerType,
                apiKeyConfigured,
                error = ex.Message,
                checkedAtUtc = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Get all AI usage costs and audit logs
    /// </summary>
    /// <returns>List of AI audit logs with cost information</returns>
    [HttpGet("costs")]
    [ProducesResponseType(typeof(IEnumerable<AiAuditLog>), 200)]
    [ProducesResponseType(403)] // Forbidden if not admin
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<AiAuditLog>>> GetAiCosts()
    {
        try
        {
            var costs = await _auditRepository.GetAiCostsAsync();
            return Ok(costs);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Failed to retrieve AI cost data",
                details = ex.Message
            });
        }
    }
    
    /// <summary>
    /// Get total AI costs grouped by model
    /// </summary>
    /// <returns>Summary of costs by AI model</returns>
    [HttpGet("costs/summary")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetAiCostsSummary()
    {
        try
        {
            var allCosts = await _auditRepository.GetAiCostsAsync();
            
            var summary = allCosts.GroupBy(log => log.ModelName)
                .Select(group => new {
                    ModelName = group.Key,
                    TotalCost = group.Sum(log => log.Cost),
                    CallCount = group.Count(),
                    AvgCostPerCall = group.Average(log => log.Cost),
                    TotalInputTokens = group.Sum(log => log.InputTokens),
                    TotalOutputTokens = group.Sum(log => log.OutputTokens)
                })
                .OrderByDescending(x => x.TotalCost)
                .ToList();
            
            var totalCost = summary.Sum(x => x.TotalCost);
            
            return Ok(new {
                TotalCost = totalCost,
                Models = summary,
                Currency = "EUR"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Failed to generate cost summary",
                details = ex.Message
            });
        }
    }
    
    /// <summary>
    /// Get AI costs for a specific time period
    /// </summary>
    /// <param name="days">Number of days to look back (default: 30)</param>
    /// <returns>AI audit logs filtered by time period</returns>
    [HttpGet("costs/recent")]
    [ProducesResponseType(typeof(IEnumerable<AiAuditLog>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<AiAuditLog>>> GetRecentAiCosts([FromQuery] int days = 30)
    {
        try
        {
            if (days <= 0 || days > 365)
            {
                return BadRequest("Days parameter must be between 1 and 365");
            }
            
            var allCosts = await _auditRepository.GetAiCostsAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var recentCosts = allCosts.Where(log => log.StartTime >= cutoffDate)
                .OrderByDescending(log => log.StartTime)
                .ToList();
            
            return Ok(recentCosts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Failed to retrieve recent AI cost data",
                details = ex.Message
            });
        }
    }
}