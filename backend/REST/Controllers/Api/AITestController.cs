using Conversey.BL.Ai;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.REST.Models.Dto;
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

    /// <summary>
    /// Beoordeel of een idee geschikt is voor publicatie.
    /// </summary>
    /// <param name="ideaDescription">De beschrijving van het idee.</param>
    /// <returns>True als het idee is goedgekeurd, anders false.</returns>
    [HttpPost("submit-idea")]
    public ActionResult<string> SubmitIdea([FromBody] IdeaSubmitionDto ideaSubmition)
    {
        try
        {
            var isAllowed = _ideaManager.SubmitIdea(ideaSubmition.Content, ideaSubmition.ForceSubmit);
            return Ok(isAllowed);
        }
        catch (InvalidSubmitionException ex)
        {
            // TODO: return text toevoegen;
            return BadRequest();
        }
    }

    /// <summary>
    /// Genereer een AI-reactie op een prompt.
    /// </summary>
    /// <param name="prompt">De prompt voor de AI.</param>
    /// <returns>De gegenereerde reactie van de AI.</returns>
    [HttpPost("generate-response")]
    public ActionResult<string> GenerateResponse([FromBody] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return BadRequest("Prompt mag niet leeg zijn.");
        }

        try
        {
            var response = _ideaManager.GenerateAiAlternative(prompt);
            return Ok(new { Response = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = $"Er is een fout opgetreden: {ex.Message}" });
        }
    }

}