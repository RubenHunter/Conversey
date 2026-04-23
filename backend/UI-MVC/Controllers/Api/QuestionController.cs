using Conversey.BL.Survey;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionManager _questionManager;

    public QuestionController(IQuestionManager questionManager)
    {
        _questionManager = questionManager;
    }

    [HttpGet("questions")]
    public ActionResult<IReadOnlyCollection<QuestionDto>> GetQuestions(Slug workspaceId, Slug projectId)
    {
        try
        {
            var dtos = _questionManager.GetQuestions(workspaceId, projectId)
                .Select(question => QuestionDto.From(question, projectId))
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
    public ActionResult SubmitAnswers(Slug workspaceId, Slug projectId, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        try
        {
            _questionManager.SubmitAnswers(
                workspaceId,
                projectId,
                submission.YouthId,
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
    public ActionResult SubmitResponsesAlias(Slug workspaceId, Slug projectId, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        return SubmitAnswers(workspaceId, projectId, submission);
    }
}
