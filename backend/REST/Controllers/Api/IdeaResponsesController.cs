using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceSlug}/projects/{projectSlug}/topics/{topicId}/ideas/{ideaId}/responses")]
public class IdeaResponsesController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectManager _projectManager;

    public IdeaResponsesController(IIdeaManager ideaManager, IProjectManager projectManager)
    {
        _ideaManager = ideaManager;
        _projectManager = projectManager;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<ResponseDto>> GetResponses(string workspaceSlug, string projectSlug, int topicId, int ideaId, [FromQuery] string? youthToken = null)
    {
        try
        {
            _ = GetIdeaForRoute(workspaceSlug, projectSlug, topicId, ideaId);
            var normalizedToken = string.IsNullOrWhiteSpace(youthToken) ? null : youthToken.Trim();

            var responses = _ideaManager.GetResponsesFromIdeaByIdeaId(ideaId)
                .Where(response => response.Status == IdeaStatus.Approved ||
                                   (normalizedToken != null && string.Equals(response.Youth.Token, normalizedToken, StringComparison.Ordinal)))
                .Select(ResponseDto.From)
                .ToList()
                .AsReadOnly();

            return Ok(responses);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
        catch (IdeaNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public ActionResult<ResponseSubmissionResponseDto> AddResponse(string workspaceSlug, string projectSlug, int topicId, int ideaId, [FromBody] CreateResponseRequestDto request)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            _ = GetIdeaForRoute(project, topicId, ideaId);

            if (string.IsNullOrWhiteSpace(request.YouthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            ResolveYouth(project, request.YouthToken);

            var submission = _ideaManager.AddResponse(request.Text, ideaId, request.YouthToken.Trim());
            return Ok(submission switch
            {
                ResponseSubmissionResponse.Approved approved => new ResponseSubmissionResponseDto.Approved(ResponseDto.From(approved.Response)),
                ResponseSubmissionResponse.Pending pending => new ResponseSubmissionResponseDto.Pending(ResponseDto.From(pending.Response), pending.Suggestion),
                _ => throw new InvalidOperationException("Unknown response submission type")
            });
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
        catch (IdeaNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{responseId}")]
    public ActionResult<ResponseDto> UpdateAfterSafetyReview(
        string workspaceSlug,
        string projectSlug,
        int topicId,
        int ideaId,
        int responseId,
        [FromBody] UpdateResponseAfterSafetyReviewDto request)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            _ = GetIdeaForRoute(project, topicId, ideaId);

            if (string.IsNullOrWhiteSpace(request.YouthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Text is required.");
            }

            var response = _ideaManager.GetResponseByIdWithIdea(responseId);
            if (response.Idea.Id != ideaId)
            {
                return NotFound();
            }

            if (!string.Equals(response.Youth.Token, request.YouthToken.Trim(), StringComparison.Ordinal))
            {
                return Forbid();
            }

            response.Text = request.Text.Trim();
            response.Status = request.MarkForReview ? IdeaStatus.Pending : IdeaStatus.Approved;
            var updated = _ideaManager.ChangeResponse(response);
            return Ok(ResponseDto.From(updated));
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
        catch (IdeaNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ResponseNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{responseId}/reactions")]
    public ActionResult<IReadOnlyCollection<ResponseReactionSummaryDto>> GetReactionSummary(string workspaceSlug, string projectSlug, int topicId, int ideaId, int responseId)
    {
        try
        {
            _ = GetResponseForRoute(workspaceSlug, projectSlug, topicId, ideaId, responseId);
            var reactions = ResponseReactionSummaryDto.From(_ideaManager.GetResponseReactionsFromResponseByResponseId(responseId));
            return Ok(reactions);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
        catch (IdeaNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ResponseNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("{responseId}/reactions")]
    public ActionResult<IReadOnlyCollection<ResponseReactionSummaryDto>> AddReaction(string workspaceSlug, string projectSlug, int topicId, int ideaId, int responseId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            _ = GetResponseForRoute(project, topicId, ideaId, responseId);

            if (string.IsNullOrWhiteSpace(request.YouthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            ResolveYouth(project, request.YouthToken);
            _ideaManager.AddResponseReaction(request.Emoji, responseId, request.YouthToken.Trim());

            return Ok(ResponseReactionSummaryDto.From(_ideaManager.GetResponseReactionsFromResponseByResponseId(responseId)));
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
        catch (IdeaNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ResponseNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{responseId}/reactions")]
    public ActionResult RemoveReaction(string workspaceSlug, string projectSlug, int topicId, int ideaId, int responseId, [FromQuery] string youthToken, [FromQuery] string emoji)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(youthToken) || string.IsNullOrWhiteSpace(emoji))
            {
                return BadRequest("youthToken and emoji are required.");
            }

            _ = GetResponseForRoute(workspaceSlug, projectSlug, topicId, ideaId, responseId);
            _ideaManager.RemoveResponseReaction(responseId, youthToken.Trim(), emoji);
            return NoContent();
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
        catch (IdeaNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ResponseNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ResponseReactionNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    private Project GetProjectForWorkspace(string workspaceSlug, string projectSlug)
    {
        var project = _projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ToSlug(projectSlug));

        if (!string.Equals(project.Workspace.Slug.Text, workspaceSlug, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProjectNotFoundException($"{workspaceSlug}/{projectSlug}");
        }

        return project;
    }

    private Idea GetIdeaForRoute(string workspaceSlug, string projectSlug, int topicId, int ideaId)
    {
        var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
        return GetIdeaForRoute(project, topicId, ideaId);
    }

    private Idea GetIdeaForRoute(Project project, int topicId, int ideaId)
    {
        var idea = _ideaManager.GetIdeaById(ideaId);
        if (idea.Project.Id != project.Id || idea.Topic.Id != topicId)
        {
            throw new IdeaNotFoundException(ideaId.ToString());
        }

        return idea;
    }

    private Response GetResponseForRoute(string workspaceSlug, string projectSlug, int topicId, int ideaId, int responseId)
    {
        var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
        return GetResponseForRoute(project, topicId, ideaId, responseId);
    }

    private Response GetResponseForRoute(Project project, int topicId, int ideaId, int responseId)
    {
        _ = GetIdeaForRoute(project, topicId, ideaId);
        var response = _ideaManager.GetResponseById(responseId);
        if (response.Idea.Id != ideaId)
        {
            throw new ResponseNotFoundException(responseId.ToString());
        }

        return response;
    }

    private void ResolveYouth(Project project, string youthToken)
    {
        try
        {
            _projectManager.GetYouthByToken(youthToken.Trim());
        }
        catch (YouthNotFoundException)
        {
            _projectManager.AddYouth(youthToken.Trim(), string.Empty, project.Id);
        }
    }

    private static Slug ToSlug(string value)
    {
        return new Slug { Text = value.Trim().ToLowerInvariant() };
    }
}

