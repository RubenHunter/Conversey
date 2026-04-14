using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
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
    public ActionResult<IReadOnlyCollection<ResponseDto>> GetResponses(string workspaceSlug, string projectSlug, int topicId, int ideaId, [FromQuery] string youthToken = null)
    {
        try
        {
            _ = GetIdeaForRoute(workspaceSlug, projectSlug, topicId, ideaId);
            Guid? normalizedToken = Guid.TryParse(youthToken?.Trim(), out var parsed) ? parsed : null;

            var responses = _ideaManager.GetResponsesFromIdeaByIdeaId(ideaId)
                .Where(response => response.Status == ModerationStatus.Approved ||
                                   (normalizedToken.HasValue && response.Youth.Token == normalizedToken.Value))
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

            if (!TryParseYouthToken(request.YouthToken, out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            ResolveYouth(project, youthToken);

            var submission = _ideaManager.AddResponse(request.Text, ideaId, youthToken);
            return Ok(submission switch
            {
                ResponseSubmissionResponse.Approved approved => new ResponseSubmissionResponseDto.Approved(ResponseDto.From(approved.Response)),
                ResponseSubmissionResponse.Pending pending => new ResponseSubmissionResponseDto.Pending(ResponseDto.From(pending.Response), pending.decision),
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
    public ActionResult<ResponseDto> UpdateAfterSafetyReview(string workspaceSlug, string projectSlug, int topicId, int ideaId, int responseId, [FromBody] UpdateResponseAfterSafetyReviewDto request)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            _ = GetIdeaForRoute(project, topicId, ideaId);

            if (!TryParseYouthToken(request.YouthToken, out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            var response = _ideaManager.GetResponseByIdWithIdea(responseId);
            if (response.Idea.Id != ideaId)
            {
                return NotFound();
            }

            if (response.Youth.Token != youthToken)
            {
                return Forbid();
            }

            response.Text = request.Text.Trim();
            response.Status = request.MarkForReview ? ModerationStatus.Pending : ModerationStatus.Approved;
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

            if (!TryParseYouthToken(request.YouthToken, out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            ResolveYouth(project, youthToken);
            _ideaManager.AddResponseReaction(request.Emoji, responseId, youthToken);

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
            if (!TryParseYouthToken(youthToken, out var token) || string.IsNullOrWhiteSpace(emoji))
            {
                return BadRequest("youthToken (guid) and emoji are required.");
            }

            _ = GetResponseForRoute(workspaceSlug, projectSlug, topicId, ideaId, responseId);
            _ideaManager.RemoveResponseReaction(responseId, token, emoji);
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
    }

    private Project GetProjectForWorkspace(string workspaceSlug, string projectSlug)
    {
        var project = _projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ToSlug(projectSlug));

        if (!string.Equals(project.Workspace.Id.Text, workspaceSlug, StringComparison.OrdinalIgnoreCase))
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
        if (idea.Project.Slug != project.Slug || idea.Topic.Id != topicId)
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

    private void ResolveYouth(Project project, Guid youthToken)
    {
        try
        {
            _projectManager.GetYouthByToken(youthToken);
        }
        catch (YouthNotFoundException)
        {
            _projectManager.AddYouth(youthToken, null, project.Slug);
        }
    }

    private static bool TryParseYouthToken(string token, out Guid parsed)
    {
        return Guid.TryParse(token?.Trim(), out parsed);
    }

    private static Slug ToSlug(string value)
    {
        return new Slug { Text = value.Trim().ToLowerInvariant() };
    }
}

