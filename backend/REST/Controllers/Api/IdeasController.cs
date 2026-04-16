using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Ideation;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/topics/{topicId:int}/ideas")]
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
    public ActionResult<SubmissionResponseDto> Submit(string workspaceId, string projectId, int topicId, [FromBody] IdeaDto idea)
    {
        try
        {
            if (idea.YouthId == Guid.Empty)
            {
                return BadRequest("YouthId must be a valid GUID.");
            }

            SubmissionResponse response = _manager.SubmitIdea(idea.Content, ProjectController.ToSlug(projectId), topicId, idea.YouthId);
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
    public ActionResult<IEnumerable<IdeaDto>> GetAllIdeasOfTopic(string workspaceId, string projectId, int topicId)
    {
        try
        {
            IEnumerable<Idea> ideas = _manager.GetIdeasByProjectIdAndTopicId(ProjectController.ToSlug(projectId), topicId);
            IEnumerable<IdeaDto> dtos = ideas
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

    [HttpGet("{ideaId:int}")]
    public ActionResult<IdeaDto> GetIdeaById(string workspaceId, string projectId, int topicId, int ideaId)
    {
        try
        {
            var idea = _manager.GetIdeaById(ProjectController.ToSlug(workspaceId), ProjectController.ToSlug(projectId), topicId, ideaId);

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

    [HttpGet("{ideaId:int}/thread")]
    public ActionResult<IdeaThreadDto> GetIdeaThread(string workspaceId, string projectId, int topicId, int ideaId)
    {
        try
        {
            var idea = _manager.GetIdeaByIdWithProjectAndResponses(ideaId);

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

    [HttpGet("{ideaId:int}/reactions")]
    public ActionResult<IEnumerable<ReactionDto>> GetIdeaReactionSummary(string workspaceId, string projectId, int topicId, int ideaId)
    {
        try
        {
            var reactions = _manager.GetIdeaReactionsByIdeaId(ProjectController.ToSlug(workspaceId), ProjectController.ToSlug(projectId), topicId, ideaId)
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
    }

    [HttpPost("{ideaId:int}/reactions")]
    public ActionResult<IReadOnlyCollection<ReactionDto>> AddIdeaReaction(string workspaceId, string projectId, int topicId, int ideaId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.YouthToken) || string.IsNullOrWhiteSpace(request.Emoji))
            {
                return BadRequest("YouthToken and emoji are required.");
            }

            if (!Guid.TryParse(request.YouthToken.Trim(), out var youthToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            var project = ProjectController.ResolveProjectForWorkspace(_projectManager, workspaceId, projectId);
            ResolveYouth(project, youthToken);

            _manager.AddIdeaReaction(request.Emoji, ideaId, request.YouthToken.Trim());
            var reactions = _manager.GetIdeaReactionsByIdeaId(ProjectController.ToSlug(workspaceId), ProjectController.ToSlug(projectId), topicId, ideaId)
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
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
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

    [HttpDelete("{ideaId:int}/reactions")]
    public ActionResult RemoveIdeaReaction(string workspaceId, string projectId, int topicId, int ideaId, [FromQuery] string youthToken, [FromQuery] string emoji)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(youthToken) || string.IsNullOrWhiteSpace(emoji))
            {
                return BadRequest("youthToken and emoji are required.");
            }

            if (!Guid.TryParse(youthToken.Trim(), out var youthId))
            {
                return BadRequest("youthToken must be a valid GUID.");
            }

            var reaction = _manager.GetIdeaReactionsByIdeaId(ProjectController.ToSlug(workspaceId), ProjectController.ToSlug(projectId), topicId, ideaId)
                .SingleOrDefault(r => r.Youth?.Id == youthId && string.Equals(r.Emoji, emoji, StringComparison.Ordinal));
            if (reaction == null)
            {
                return NotFound();
            }

            _manager.RemoveIdeaReaction(ProjectController.ToSlug(workspaceId), ProjectController.ToSlug(projectId), topicId, ideaId, youthId, reaction.Id);
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

    [HttpDelete("{ideaId:int}/reactions/{reactionId:int}")]
    public ActionResult RemoveIdeaReaction(string workspaceId, string projectId, int topicId, int ideaId, [FromQuery] Guid youthId, int reactionId)
    {
        try
        {
            _manager.RemoveIdeaReaction(ProjectController.ToSlug(workspaceId), ProjectController.ToSlug(projectId), topicId, ideaId, youthId, reactionId);
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

    [HttpPut("{ideaId:int}")]
    public ActionResult<IdeaDto> UpdateAfterSafetyReview(
        string workspaceId,
        string projectId,
        int topicId,
        int ideaId,
        [FromBody] UpdateIdeaAfterSafetyReviewDto request)
    {
        try
        {
            var idea = _manager.GetIdeaById(ProjectController.ToSlug(workspaceId), ProjectController.ToSlug(projectId), topicId, ideaId);
            idea.Status = request.MarkForReview ? ModerationStatus.Pending : ModerationStatus.Approved;
            idea.Content = request.Content;
            var updated = _manager.ChangeIdea(ProjectController.ToSlug(workspaceId), ProjectController.ToSlug(projectId), topicId, idea);

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
}
