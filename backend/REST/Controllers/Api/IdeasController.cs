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
[Route("api/workspaces/{workspaceSlug}/projects/{projectSlug}/topics/{topicId}/ideas")]
public class IdeasController : ControllerBase
{
    private readonly IIdeaManager _manager;
    private readonly IProjectManager _projectManager;

    public IdeasController(IIdeaManager manager, IProjectManager projectManager)
    {
        _manager = manager;
        _projectManager = projectManager;
    }

    [HttpPost]
    public ActionResult<SubmissionResponseDto> Submit(string workspaceSlug, string projectSlug, int topicId, [FromBody] IdeaDto idea)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            if (idea.TopicId != topicId)
            {
                return BadRequest("TopicId in payload does not match route topic.");
            }

            if (!TopicBelongsToProject(project, topicId))
            {
                return NotFound();
            }

            if (!TryParseYouthToken(idea.YouthToken, out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            ResolveYouth(project, youthToken);

            SubmissionResponse response = _manager.SubmitIdea(idea.Content, project.Slug, topicId, youthToken);
            return Ok(response switch
            {
                SubmissionResponse.Approved approved => new SubmissionResponseDto.Approved(IdeaDto.From(approved.idea)),
                SubmissionResponse.Pending pending => new SubmissionResponseDto.Pending(IdeaDto.From(pending.idea), pending.decision),
                _ => throw new InvalidOperationException("Unknown submission response type")
            });
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

    [HttpGet]
    public ActionResult<IReadOnlyCollection<IdeaDto>> GetAllIdeasOfTopic(string workspaceSlug, string projectSlug, int topicId)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            if (!TopicBelongsToProject(project, topicId))
            {
                return NotFound();
            }

            var ideas = _manager.GetIdeasFromTopicByProjectSlugAndTopicId(project.Slug, topicId)
                .Select(IdeaDto.From)
                .ToList()
                .AsReadOnly();

            return Ok(ideas);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{ideaId}")]
    public ActionResult<IdeaDto> GetIdeaById(string workspaceSlug, string projectSlug, int topicId, int ideaId)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            var idea = _manager.GetIdeaById(ideaId);
            if (idea.Project.Slug != project.Slug || idea.Topic.Id != topicId)
            {
                return NotFound();
            }

            return Ok(IdeaDto.From(idea));
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

    [HttpGet("{ideaId}/thread")]
    public ActionResult<IdeaThreadDto> GetIdeaThread(string workspaceSlug, string projectSlug, int topicId, int ideaId)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            var idea = _manager.GetIdeaByIdWithProjectAndResponses(ideaId);
            if (idea.Project.Slug != project.Slug || idea.Topic.Id != topicId)
            {
                return NotFound();
            }

            return Ok(IdeaThreadDto.From(idea));
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

    [HttpGet("{ideaId}/reactions")]
    public ActionResult<IReadOnlyCollection<ResponseReactionSummaryDto>> GetIdeaReactionSummary(string workspaceSlug, string projectSlug, int topicId, int ideaId)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            var idea = _manager.GetIdeaById(ideaId);
            if (idea.Project.Slug != project.Slug || idea.Topic.Id != topicId)
            {
                return NotFound();
            }

            return Ok(ResponseReactionSummaryDto.From(_manager.GetIdeaReactionsFromIdeaByIdeaId(ideaId)));
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

    [HttpPost("{ideaId}/reactions")]
    public ActionResult<IReadOnlyCollection<ResponseReactionSummaryDto>> AddIdeaReaction(string workspaceSlug, string projectSlug, int topicId, int ideaId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            var idea = _manager.GetIdeaById(ideaId);
            if (idea.Project.Slug != project.Slug || idea.Topic.Id != topicId)
            {
                return NotFound();
            }

            if (!TryParseYouthToken(request.YouthToken, out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            ResolveYouth(project, youthToken);
            _manager.AddIdeaReaction(request.Emoji, ideaId, youthToken);
            return Ok(ResponseReactionSummaryDto.From(_manager.GetIdeaReactionsFromIdeaByIdeaId(ideaId)));
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

    [HttpDelete("{ideaId}/reactions")]
    public ActionResult RemoveIdeaReaction(string workspaceSlug, string projectSlug, int topicId, int ideaId, [FromQuery] string youthToken, [FromQuery] string emoji)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            var idea = _manager.GetIdeaById(ideaId);
            if (idea.Project.Slug != project.Slug || idea.Topic.Id != topicId)
            {
                return NotFound();
            }

            if (!TryParseYouthToken(youthToken, out var token) || string.IsNullOrWhiteSpace(emoji))
            {
                return BadRequest("youthToken (guid) and emoji are required.");
            }

            _manager.RemoveIdeaReaction(ideaId, token, emoji);
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
        catch (IdeaReactionNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut("{ideaId}")]
    public ActionResult<IdeaDto> UpdateAfterSafetyReview(string workspaceSlug, string projectSlug, int topicId, int ideaId, [FromBody] UpdateIdeaAfterSafetyReviewDto request)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            if (!TryParseYouthToken(request.YouthToken, out var token))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            var idea = _manager.GetIdeaByIdWithProject(ideaId);
            if (idea.Project.Slug != project.Slug || idea.Topic.Id != topicId)
            {
                return NotFound();
            }

            if (idea.Youth.Token != token)
            {
                return Forbid();
            }

            idea.Content = request.Content.Trim();
            idea.Status = request.MarkForReview ? ModerationStatus.Pending : ModerationStatus.Approved;
            var updated = _manager.ChangeIdea(idea);
            return Ok(IdeaDto.From(updated));
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

    private Project GetProjectForWorkspace(string workspaceSlug, string projectSlug)
    {
        var project = _projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ToSlug(projectSlug));
        if (!string.Equals(project.Workspace.Id.Text, workspaceSlug, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProjectNotFoundException($"{workspaceSlug}/{projectSlug}");
        }

        return project;
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

    private static bool TopicBelongsToProject(Project project, int topicId)
    {
        return (project.Topic ?? Array.Empty<Topic>()).Any(topic => topic.Id == topicId);
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
