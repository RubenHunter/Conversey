using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Ideation;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using IdeaResponse = Conversey.BL.Domain.Ideation.Response;

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
            // _ = is for exceptions checking.
            _ = GetIdeaForRoute(workspaceSlug, projectSlug, topicId, ideaId);
            Guid? normalizedToken = null;
            if (!string.IsNullOrWhiteSpace(youthToken))
            {
                if (!Guid.TryParse(youthToken.Trim(), out var parsedYouthToken))
                {
                    return BadRequest("YouthToken must be a valid GUID.");
                }

                normalizedToken = parsedYouthToken;
            }

            var responses = _ideaManager.GetResponsesFromIdeaByIdeaId(ideaId)
                .Where(response => response.Status == ModerationStatus.Approved ||
                                   (normalizedToken.HasValue && response.Youth.Id == normalizedToken.Value))
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
            var project = ProjectController.ResolveProjectForWorkspace(_projectManager, workspaceSlug, projectSlug);
            _ = GetIdeaForRoute(project, topicId, ideaId);

            if (string.IsNullOrWhiteSpace(request.YouthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            if (!Guid.TryParse(request.YouthToken.Trim(), out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            ResolveYouth(project, youthToken);

            var submission = _ideaManager.AddResponse(request.Text, ideaId, request.YouthToken.Trim());
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
            var project = ProjectController.ResolveProjectForWorkspace(_projectManager, workspaceSlug, projectSlug);
            _ = GetIdeaForRoute(project, topicId, ideaId);

            if (string.IsNullOrWhiteSpace(request.YouthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Text is required.");
            }

            if (!Guid.TryParse(request.YouthToken.Trim(), out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            var response = _ideaManager.GetResponseByIdWithIdea(responseId);
            if (response.Idea.Id != ideaId)
            {
                return NotFound();
            }

            if (response.Youth.Id != youthToken)
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
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{responseId}/reactions")]
    public ActionResult<IReadOnlyCollection<ReactionDto>> GetReactionSummary(string workspaceSlug, string projectSlug, int topicId, int ideaId, int responseId)
    {
        try
        {
            _ = GetResponseForRoute(workspaceSlug, projectSlug, topicId, ideaId, responseId);
            var reactions = _ideaManager.GetResponseReactionsFromResponseByResponseId(responseId)
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto { Emoji = g.Key, Count = g.Count() })
                .ToList()
                .AsReadOnly();
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
    public ActionResult<IReadOnlyCollection<ReactionDto>> AddReaction(string workspaceSlug, string projectSlug, int topicId, int ideaId, int responseId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
            var project = ProjectController.ResolveProjectForWorkspace(_projectManager, workspaceSlug, projectSlug);
            _ = GetResponseForRoute(project, topicId, ideaId, responseId);

            if (string.IsNullOrWhiteSpace(request.YouthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            if (!Guid.TryParse(request.YouthToken.Trim(), out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            ResolveYouth(project, youthToken);
            _ideaManager.AddResponseReaction(request.Emoji, responseId, request.YouthToken.Trim());

            return Ok(_ideaManager.GetResponseReactionsFromResponseByResponseId(responseId)
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto { Emoji = g.Key, Count = g.Count() })
                .ToList()
                .AsReadOnly());
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

            if (!Guid.TryParse(youthToken.Trim(), out _))
            {
                return BadRequest("youthToken must be a valid GUID.");
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

    private Idea GetIdeaForRoute(string workspaceSlug, string projectSlug, int topicId, int ideaId)
    {
        var project = ProjectController.ResolveProjectForWorkspace(_projectManager, workspaceSlug, projectSlug);
        return GetIdeaForRoute(project, topicId, ideaId);
    }

    private Idea GetIdeaForRoute(Project project, int topicId, int ideaId)
    {
        var idea = _ideaManager.GetIdeaByIdWithProjectAndResponses(ideaId);
        if (idea.Project.Id != project.Id || idea.Topic.Id != topicId)
        {
            throw new IdeaNotFoundException(ideaId);
        }

        return idea;
    }

    private IdeaResponse GetResponseForRoute(string workspaceSlug, string projectSlug, int topicId, int ideaId, int responseId)
    {
        var project = ProjectController.ResolveProjectForWorkspace(_projectManager, workspaceSlug, projectSlug);
        return GetResponseForRoute(project, topicId, ideaId, responseId);
    }

    private IdeaResponse GetResponseForRoute(Project project, int topicId, int ideaId, int responseId)
    {
        _ = GetIdeaForRoute(project, topicId, ideaId);
        var response = _ideaManager.GetResponseByIdWithIdea(responseId);
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
            var youth = _projectManager.GetYouthByToken(youthToken);
            if (youth.Project.Id != project.Id)
            {
                throw new ValidationException($"Youth '{youthToken}' does not belong to project '{project.Id.Text}'.");
            }
        }
        catch (YouthNotFoundException)
        {
            _projectManager.AddYouth(youthToken, $"{youthToken:N}@local.invalid", project.Id);
        }
    }

}

