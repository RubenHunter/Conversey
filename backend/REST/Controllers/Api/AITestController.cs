using Conversey.BL.Ai;
using Conversey.BL.Subplatform.Survey.Ideation;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/ai")]
public class AiTestController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;

    public AiTestController(IIdeaManager ideaManager)
    {
        _ideaManager = ideaManager;
    }
    
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("AI Service is running");
    }

    /// <summary>
    /// Beoordeel of een idee geschikt is voor publicatie.
    /// </summary>
    /// <param name="ideaDescription">De beschrijving van het idee.</param>
    /// <returns>True als het idee is goedgekeurd, anders false.</returns>
    [HttpPost("check-idea")]
    public async Task<ActionResult<bool>> CheckIdea([FromBody] string ideaDescription)
    {
        if (string.IsNullOrWhiteSpace(ideaDescription))
        {
            return BadRequest("Idee-beschrijving mag niet leeg zijn.");
        }

        try
        {
            bool isAllowed = await _ideaManager.IsIdeaAllowedAsync(ideaDescription);
            return Ok(new { IsAllowed = isAllowed });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Er is een fout opgetreden: {ex.Message}" });
        }
    }

    /// <summary>
    /// Genereer een AI-reactie op een prompt.
    /// </summary>
    /// <param name="prompt">De prompt voor de AI.</param>
    /// <returns>De gegenereerde reactie van de AI.</returns>
    [HttpPost("generate-response")]
    public async Task<ActionResult<string>> GenerateResponse([FromBody] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return BadRequest("Prompt mag niet leeg zijn.");
        }

        try
        {
            var response = await _ideaManager.GenerateAISuggestionAsync(prompt);
            return Ok(new { Response = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Er is een fout opgetreden: {ex.Message}" });
        }
    }
    
    [HttpPost("test-generate")]
    public async Task<ActionResult<string>> TestGenerate([FromBody] string prompt)
    {
        try
        {
            string response = await _ideaManager.GenerateAISuggestionAsync(prompt);
            return Ok(new { Response = response });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout in TestGenerate: {ex.Message}");
            return StatusCode(500, new { Error = $"Er is een fout opgetreden: {ex.Message}" });
        }
    }
    
    [HttpPost("test-direct")]
    public async Task<ActionResult<string>> TestDirect([FromServices] IAiManager aiService, [FromBody] string prompt)
    {
        try
        {
            string response = await aiService.GenerateResponseAsync(prompt);
            return Ok(new { Response = response });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout in TestDirect: {ex}");
            return StatusCode(500, new { Error = $"Er is een fout opgetreden: {ex.Message}" });
        }
    }
    
    [HttpGet("test-config")]
    public ActionResult<string> TestConfig([FromServices] IAiManager aiManager)
    {
        if (aiManager is MistralAiManager mistralAiManager)
        {
            Console.WriteLine($"API Key: {mistralAiManager.GetApiKey()}, Model: {mistralAiManager.GetModelName()}");
            return Ok(new { ApiKey = "*****", Model = mistralAiManager.GetModelName() });
        }
        return BadRequest("Kon MistralAiManager niet casten.");
    }

}