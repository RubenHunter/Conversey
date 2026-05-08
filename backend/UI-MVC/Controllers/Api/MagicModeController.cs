using Conversey.BL.Ai;
using Conversey.BL.Domain.DTOs.MagicMode;
using Microsoft.AspNetCore.Mvc;

namespace UI_MVC.Controllers.Api;

[ApiController]
[Route("api/magic-mode")]
public class MagicModeController(IAiManager aiManager) : ControllerBase
{
    [HttpPost("key-phrases")]
    public async Task<IActionResult> ExtractKeyPhrases([FromBody] ExtractKeyPhrasesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Transcript))
            return BadRequest("Transcript is required.");

        var response = await aiManager.ExtractKeyPhrases(
            request.Transcript,
            request.Language,
            request.MaxPhrases,
            request.ExistingPhrases,
            request.RejectedPhrases);
        return Ok(response);
    }
}
