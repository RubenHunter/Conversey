using Conversey.BL.Survey;
using Conversey.UI_MVC.Models.Dto;
using Conversey.UI_MVC.Security;
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
    private readonly IProjectAccessService _projectAccessService;

    public QuestionController(IQuestionManager questionManager, ILogger<QuestionController> logger, IProjectAccessService projectAccessService)
    {
        _questionManager = questionManager;
        _logger = logger;
        _projectAccessService = projectAccessService;
    }

    [HttpGet("questions")]
    public async Task<ActionResult<IReadOnlyCollection<QuestionDto>>> GetQuestions(Slug workspaceId, Slug projectId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

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
    public async Task<ActionResult> SubmitAnswers(Slug workspaceId, Slug projectId, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

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
    public async Task<ActionResult> SubmitResponsesAlias(Slug workspaceId, Slug projectId, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        return await SubmitAnswers(workspaceId, projectId, submission);
    }

    private async Task<bool> IsActiveProjectOrAdmin(Slug workspaceId, Slug projectId)
    {
        return await _projectAccessService.IsActiveProjectOrAdminAsync(workspaceId, projectId, User);
    }
}
