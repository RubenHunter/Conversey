using Conversey.BL.Domain;
using Conversey.DAL.Subplatform.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/ai")]
[Authorize(Roles = "Admin")]
public class AiController : Controller
{
    private readonly IAuditRepository _auditRepository;
    private readonly IPromptRepository _promptRepository;
    
    public AiController(IAuditRepository auditRepository, IPromptRepository promptRepository)
    {
        _auditRepository = auditRepository;
        _promptRepository = promptRepository;
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

    // ===== PROMPT MANAGEMENT ENDPOINTS =====

    /// <summary>
    /// Get all AI prompts
    /// </summary>
    /// <returns>List of all AI prompts</returns>
    [HttpGet("prompts")]
    [ProducesResponseType(typeof(IEnumerable<AiPrompt>), 200)]
    [ProducesResponseType(403)] // Forbidden if not admin
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<AiPrompt>>> GetAllPrompts()
    {
        try
        {
            var prompts = await _promptRepository.ReadAllPromptsAsync();
            return Ok(prompts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Failed to retrieve prompts",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Get a specific prompt by key
    /// </summary>
    /// <param name="key">The prompt key</param>
    /// <returns>The requested prompt</returns>
    [HttpGet("prompts/{key}")]
    [ProducesResponseType(typeof(AiPrompt), 200)]
    [ProducesResponseType(404)] // Not found
    [ProducesResponseType(403)] // Forbidden if not admin
    [ProducesResponseType(500)]
    public async Task<ActionResult<AiPrompt>> GetPrompt(string key)
    {
        try
        {
            var prompt = await _promptRepository.ReadPromptByKeyAsync(key);
            
            if (prompt == null)
            {
                return NotFound(new { error = "Prompt not found" });
            }
            
            return Ok(prompt);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Failed to retrieve prompt",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Create a new AI prompt
    /// </summary>
    /// <param name="prompt">The prompt to create</param>
    /// <returns>The created prompt</returns>
    [HttpPost("prompts")]
    [ProducesResponseType(typeof(AiPrompt), 201)] // Created
    [ProducesResponseType(400)] // Bad request
    [ProducesResponseType(403)] // Forbidden if not admin
    [ProducesResponseType(500)]
    public async Task<ActionResult<AiPrompt>> CreatePrompt([FromBody] AiPrompt prompt)
    {
        try
        {
            if (prompt == null)
            {
                return BadRequest(new { error = "Prompt data is required" });
            }

            if (string.IsNullOrWhiteSpace(prompt.Key))
            {
                return BadRequest(new { error = "Prompt key is required" });
            }

            if (string.IsNullOrWhiteSpace(prompt.Template))
            {
                return BadRequest(new { error = "Prompt template is required" });
            }

            var createdPrompt = await _promptRepository.CreatePromptAsync(prompt);
            
            return CreatedAtAction(nameof(GetPrompt), new { key = createdPrompt.Key }, createdPrompt);
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Failed to create prompt",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Update an existing AI prompt
    /// </summary>
    /// <param name="id">The prompt ID</param>
    /// <param name="prompt">The updated prompt data</param>
    /// <returns>The updated prompt</returns>
    [HttpPut("prompts/{id}")]
    [ProducesResponseType(typeof(AiPrompt), 200)]
    [ProducesResponseType(400)] // Bad request
    [ProducesResponseType(404)] // Not found
    [ProducesResponseType(403)] // Forbidden if not admin
    [ProducesResponseType(500)]
    public async Task<ActionResult<AiPrompt>> UpdatePrompt(int id, [FromBody] AiPrompt prompt)
    {
        try
        {
            if (prompt == null)
            {
                return BadRequest(new { error = "Prompt data is required" });
            }

            if (string.IsNullOrWhiteSpace(prompt.Key))
            {
                return BadRequest(new { error = "Prompt key is required" });
            }

            if (string.IsNullOrWhiteSpace(prompt.Template))
            {
                return BadRequest(new { error = "Prompt template is required" });
            }

            var updatedPrompt = await _promptRepository.UpdatePromptAsync(id, prompt);
            
            if (updatedPrompt == null)
            {
                return NotFound(new { error = "Prompt not found" });
            }
            
            return Ok(updatedPrompt);
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Failed to update prompt",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete an AI prompt
    /// </summary>
    /// <param name="id">The prompt ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("prompts/{id}")]
    [ProducesResponseType(204)] // No content
    [ProducesResponseType(404)] // Not found
    [ProducesResponseType(403)] // Forbidden if not admin
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeletePrompt(int id)
    {
        try
        {
            var success = await _promptRepository.DeletePromptAsync(id);
            
            if (!success)
            {
                return NotFound(new { error = "Prompt not found" });
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Failed to delete prompt",
                details = ex.Message
            });
        }
    }
}