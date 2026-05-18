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

    [HttpPost("generate-text")]
    public async Task<IActionResult> GenerateText([FromBody] GenerateTextFromBubblesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Transcript) || request.Bubbles == null || request.Bubbles.Count == 0)
            return BadRequest("Transcript and bubbles are required.");

        var text = await aiManager.GenerateTextFromBubbles(
            request.Transcript,
            request.Bubbles,
            request.Language,
            request.RejectedPhrases);
        
        if (string.IsNullOrWhiteSpace(text))
            return Ok(new { Text = string.Empty });
        
        return Ok(new { Text = text });
    }
}
