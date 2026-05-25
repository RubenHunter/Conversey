using Conversey.BL.Survey;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}")]
public class QuestionController : ControllerBase
{
    private readonly IQuestionManager _questionManager;
    private readonly ILogger<QuestionController> _logger;

    public QuestionController(IQuestionManager questionManager, ILogger<QuestionController> logger)
    {
        _questionManager = questionManager;
        _logger = logger;
    }

    [HttpGet("questions")]
    public ActionResult<IReadOnlyCollection<QuestionDto>> GetQuestions(Slug workspaceId, Slug projectId)
    {
        try
        {
            var dtos = _questionManager.GetQuestions(workspaceId, projectId)
                .Select(question => QuestionDto.From(question))
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
            _logger.LogWarning("Survey answer submission validation failed for project {ProjectId}: {Message}", projectId, e.Message);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error during survey answer submission for project {ProjectId}", projectId);
            throw;
        }
    }

    // Backward-compatible alias while frontend switches from /responses to /answers.
    [HttpPost("responses")]
    public ActionResult SubmitResponsesAlias(Slug workspaceId, Slug projectId, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        return SubmitAnswers(workspaceId, projectId, submission);
    }
}
