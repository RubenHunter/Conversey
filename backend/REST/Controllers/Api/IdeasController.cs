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
            if (idea.ProjectId != project.Id)
            {
                return BadRequest("ProjectId in payload does not match route project.");
            }

            if (idea.TopicId != topicId)
            {
                return BadRequest("TopicId in payload does not match route topic.");
            }

            if (!TopicBelongsToProject(project, topicId))
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(idea.YouthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            ResolveYouth(project, idea.YouthToken);

            SubmissionResponse response = _manager.SubmitIdea(idea.Content, project.Id, topicId, idea.YouthToken.Trim());
            return Ok(response switch
            {
                SubmissionResponse.Approved approved => new SubmissionResponseDto.Approved(IdeaDto.From(approved.idea)),
                SubmissionResponse.Pending pending => new SubmissionResponseDto.Pending(IdeaDto.From(pending.idea), pending.suggestion),
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

            IReadOnlyCollection<Idea> ideas = _manager.GetIdeasFromTopicByProjectSlugAndTopicId(project.Slug, topicId);
            IReadOnlyCollection<IdeaDto> dtos = ideas
                .Select(IdeaDto.From)
                .ToList()
                .AsReadOnly();

            return Ok(dtos);
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
            if (idea.Project.Id != project.Id || idea.Topic.Id != topicId)
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
            if (idea.Project.Id != project.Id || idea.Topic.Id != topicId)
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
            if (idea.Project.Id != project.Id || idea.Topic.Id != topicId)
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
            if (idea.Project.Id != project.Id || idea.Topic.Id != topicId)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.YouthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            ResolveYouth(project, request.YouthToken);
            _manager.AddIdeaReaction(request.Emoji, ideaId, request.YouthToken.Trim());
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
            if (idea.Project.Id != project.Id || idea.Topic.Id != topicId)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(youthToken) || string.IsNullOrWhiteSpace(emoji))
            {
                return BadRequest("youthToken and emoji are required.");
            }

            _manager.RemoveIdeaReaction(ideaId, youthToken.Trim(), emoji);
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

    private static bool TopicBelongsToProject(Project project, int topicId)
    {
        return (project.Topic ?? Array.Empty<Topic>()).Any(topic => topic.Id == topicId);
    }

    private static Slug ToSlug(string value)
    {
        return new Slug { Text = value.Trim().ToLowerInvariant() };
    }
}
