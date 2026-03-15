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
    public ActionResult<IReadOnlyCollection<ResponseDto>> GetResponses(string workspaceSlug, string projectSlug, int topicId, int ideaId)
    {
        try
        {
            _ = GetIdeaForRoute(workspaceSlug, projectSlug, topicId, ideaId);
            var responses = _ideaManager.GetResponsesFromIdeaByIdeaId(ideaId)
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
    public ActionResult<ResponseDto> AddResponse(string workspaceSlug, string projectSlug, int topicId, int ideaId, [FromBody] CreateResponseRequestDto request)
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
            var response = _ideaManager.AddResponse(request.Text, ideaId, request.YouthToken.Trim());
            return Ok(ResponseDto.From(response));
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
        if (idea.ProjectId != project.Id || idea.TopicId != topicId)
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
        if (response.IdeaId != ideaId)
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

