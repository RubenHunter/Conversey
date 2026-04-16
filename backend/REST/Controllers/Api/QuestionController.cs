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
    public ActionResult<IReadOnlyCollection<QuestionDto>> GetQuestions(string workspaceSlug, string projectSlug)
    {
        try
        {
            var normalizedWorkspaceSlug = ProjectController.ToSlug(workspaceSlug);
            var normalizedProjectSlug = ProjectController.ToSlug(projectSlug);
            var dtos = _questionManager.GetQuestions(normalizedWorkspaceSlug, normalizedProjectSlug)
                .Select(question => QuestionDto.From(question, normalizedProjectSlug.Text))
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
    public ActionResult SubmitAnswers(string workspaceSlug, string projectSlug, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        try
        {
            var normalizedWorkspaceSlug = ProjectController.ToSlug(workspaceSlug);
            var normalizedProjectSlug = ProjectController.ToSlug(projectSlug);
            _questionManager.SubmitAnswers(
                normalizedWorkspaceSlug,
                normalizedProjectSlug,
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
    public ActionResult SubmitResponsesAlias(string workspaceSlug, string projectSlug, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        return SubmitAnswers(workspaceSlug, projectSlug, submission);
    }
}
