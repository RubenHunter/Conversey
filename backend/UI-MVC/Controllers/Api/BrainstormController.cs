using System.Text.RegularExpressions;
using Conversey.BL.Ai;
using Conversey.BL.Ai.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace UI_MVC.Controllers.Api;

[ApiController]
[Route("api/brainstorm")]
public class BrainstormController(IAiManager aiManager, ILogger<BrainstormController> logger) : ControllerBase
{
    [HttpPost("key-phrases")]
    public async Task<IActionResult> ExtractKeyPhrases([FromBody] ExtractKeyPhrasesRequest request)
    {
        try
        {
            var response = await aiManager.ExtractKeyPhrases(
                request.Transcript,
                request.Language,
                request.MaxPhrases,
                request.ExistingPhrases,
                request.RejectedPhrases);
            return Ok(response);
        }
        catch (AiException ex)
        {
            return HandleAiException(ex);
        }
    }

    [HttpPost("generate-text")]
    public async Task<IActionResult> GenerateText([FromBody] GenerateTextFromBubblesRequest request)
    {
        try
        {
            var text = await aiManager.GenerateTextFromBubbles(
                request.Transcript,
                request.Bubbles,
                request.Language,
                request.RejectedPhrases);

            if (string.IsNullOrWhiteSpace(text))
                return Ok(new { Text = string.Empty });

            return Ok(new { Text = text });
        }
        catch (AiException ex)
        {
            return HandleAiException(ex);
        }
    }

    private IActionResult HandleAiException(AiException ex)
    {
        var match = Regex.Match(ex.Message, @"\((\d+)\)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var statusCode))
        {
            logger.LogWarning(ex, "AI provider returned {StatusCode}", statusCode);
            return StatusCode(statusCode, new { error = ex.Message });
        }

        logger.LogError(ex, "Brainstorm AI error");
        return StatusCode(502, new { error = "AI service unavailable" });
    }
}
