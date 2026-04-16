using Conversey.BL.Domain.Common;
using Conversey.BL.Survey;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceSlug}/projects/{projectSlug}")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionManager _questionManager;

    public QuestionController(IQuestionManager questionManager)
    {
        _questionManager = questionManager;
    }

    [HttpGet("questions")]
    public ActionResult<IReadOnlyCollection<QuestionDto>> GetQuestions(Slug workspaceSlug, Slug projectSlug)
    {
        try
        {
            var dtos = _questionManager.GetQuestions(workspaceSlug, projectSlug)
                .Select(question => QuestionDto.From(question, projectSlug.Text))
                .ToList()
                .AsReadOnly();

            return Ok(dtos);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("answers")]
    public ActionResult SubmitAnswers(Slug workspaceSlug, Slug projectSlug, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        try
        {
            _questionManager.SubmitAnswers(
                workspaceSlug,
                projectSlug,
                submission.YouthId ?? string.Empty,
                submission.Answers.Select(answer => (
                    answer.QuestionId,
                    answer.SelectedOptionId,
                    answer.OpenTextValue ?? string.Empty)));

            return NoContent();
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    // Backward-compatible alias while frontend switches from /responses to /answers.
    [HttpPost("responses")]
    public ActionResult SubmitResponsesAlias(Slug workspaceSlug, Slug projectSlug, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        return SubmitAnswers(workspaceSlug, projectSlug, submission);
    }
}
