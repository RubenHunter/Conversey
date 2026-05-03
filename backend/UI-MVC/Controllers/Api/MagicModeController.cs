using Conversey.BL.Ai;
using Microsoft.AspNetCore.Mvc;
using UI_MVC.DTOs.MagicMode;

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

        var phrases = await aiManager.ExtractKeyPhrases(
            request.Transcript,
            request.Language,
            request.MaxPhrases,
            request.ExistingPhrases,
            request.RejectedPhrases);
        return Ok(new ExtractKeyPhrasesResponse(phrases));
    }
}
