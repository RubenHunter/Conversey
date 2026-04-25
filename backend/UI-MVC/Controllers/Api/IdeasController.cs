using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Ideation;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/topics/{topicId:int}/ideas")]
public class IdeasController : ControllerBase
{
    private readonly IIdeaManager _manager;

    public IdeasController(IIdeaManager manager)
    {
        _manager = manager;
    }

    [HttpPost]
    public ActionResult<SubmissionResponseDto> Submit(Slug workspaceId, Slug projectId, int topicId, [FromBody] IdeaDto idea)
    {
        try
        {
            SubmissionResponse response = _manager.SubmitIdea(workspaceId, projectId, topicId, idea.YouthId, idea.Content);
            return Ok(response switch
            {
                SubmissionResponse.Approved approved => new SubmissionResponseDto.Approved(IdeaDto.From(approved.Idea)),
                SubmissionResponse.Pending pending => new SubmissionResponseDto.Pending(IdeaDto.From(pending.Idea), pending.Decision),
                _ => throw new InvalidOperationException("Unknown submission response type")
            });
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet]
    public ActionResult<IEnumerable<IdeaDto>> GetAllIdeasOfTopic(Slug workspaceId, Slug projectId, int topicId)
    {
        try
        {
            IEnumerable<Idea> ideas = _manager.GetIdeasByProjectIdAndTopicId(workspaceId, projectId, topicId);
            IEnumerable<IdeaDto> dtos = ideas
                .Select(IdeaDto.From)
                .ToList()
                .AsReadOnly(); 

            return Ok(dtos);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("discover")]
    public ActionResult<IEnumerable<IdeaDto>> DiscoverIdeas(
        Slug workspaceId,
        Slug projectId,
        int topicId,
        [FromQuery] Guid youthId,
        [FromQuery] IdeaDiscoveryCategory category = IdeaDiscoveryCategory.Random,
        [FromQuery] int limit = 30)
    {
        if (youthId == Guid.Empty)
        {
            return BadRequest("youthId is required.");
        }
        int boundedLimit = Math.Clamp(limit, 1, 30);

        try
        {
            var ideas = _manager.GetIdeaDiscoverySuggestions(
                workspaceId,
                projectId,
                topicId,
                youthId,
                category,
                boundedLimit);

            return Ok(ideas.Select(IdeaDto.From).ToList().AsReadOnly());
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("{ideaId:int}")]
    public ActionResult<IdeaDto> GetIdeaById(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        try
        {
            var idea = _manager.GetIdeaById(workspaceId, projectId, topicId, ideaId);

            return Ok(IdeaDto.From(idea));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("{ideaId:int}/thread")]
    public ActionResult<IdeaThreadDto> GetIdeaThread(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        try
        {
            var idea = _manager.GetIdeaByIdWithProjectAndResponses(workspaceId, projectId, topicId, ideaId);

            return Ok(IdeaThreadDto.From(idea));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("{ideaId:int}/reactions")]
    public ActionResult<IEnumerable<ReactionDto>> GetIdeaReactionSummary(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        try
        {
            var reactions = _manager.GetIdeaReactionsByIdeaId(workspaceId, projectId, topicId, ideaId)
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionDto { Emoji = g.Key, Count = g.Count() })
                .ToList()
                .AsReadOnly();
            return Ok(reactions);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("{ideaId:int}/reactions")]
    public ActionResult<CreatedReactionDto> AddIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
            var addedReaction = _manager.AddIdeaReaction(workspaceId, projectId, topicId, ideaId, request.YouthId, request.Emoji);
            return Ok(new CreatedReactionDto
            {
                Id = addedReaction.Id,
                Emoji = addedReaction.Emoji
            });
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpDelete("{ideaId:int}/reactions/{reactionId:int}")]
    public ActionResult RemoveIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromQuery] Guid youthId, int reactionId)
    {
        try
        {
            _manager.RemoveIdeaReaction(workspaceId, projectId, topicId, ideaId, youthId, reactionId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut("{ideaId:int}")]
    public ActionResult<IdeaDto> UpdateAfterSafetyReview(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromBody] UpdateIdeaAfterSafetyReviewDto request)
    {
        try
        {
            ModerationStatus newStatus = request.MarkForReview ? ModerationStatus.Pending : ModerationStatus.Approved;
            var updated = _manager.ChangeIdea(workspaceId, projectId, topicId, ideaId, newStatus, request.Content);
            
            return StatusCode(201, IdeaDto.From(updated));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}
