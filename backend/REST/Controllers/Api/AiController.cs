using Conversey.DAL.Subplatform.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/ai")]
//[Authorize(Roles = "Admin")]
public class AiController : Controller
{
    private readonly IAuditRepository _auditRepository;
    
    public AiController(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }
    
    [Authorize]
    [HttpGet("ai/costs")]
    public async Task<IActionResult> GetAiCosts()
    {
        var costs = await _auditRepository.GetAICostsAsync();
        return Ok(costs);
    }
}