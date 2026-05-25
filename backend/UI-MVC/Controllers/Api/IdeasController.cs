using Conversey.BL.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Ideation;
using Conversey.UI_MVC.Models.Dto;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/topics/{topicId:int}/ideas")]
[EnableRateLimiting("AiFixedPolicy")]
public class IdeasController : ControllerBase
{
    private readonly IIdeaManager _manager;
    private readonly IProjectAccessService _projectAccessService;

    public IdeasController(IIdeaManager manager, IProjectAccessService projectAccessService)
    {
        _manager = manager;
        _projectAccessService = projectAccessService;
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionResponseDto>> Submit(Slug workspaceId, Slug projectId, int topicId, [FromBody] IdeaDto idea)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            SubmissionResponse response = await _manager.SubmitIdeaAsync(workspaceId, projectId, topicId, idea.YouthId, idea.Content, idea.QualityNudgeBypassed);
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

    [HttpPost("nudge")]
    public async Task<ActionResult<IdeaNudgeResponseDto>> AssessNudging(Slug workspaceId, Slug projectId, int topicId, [FromBody] IdeaNudgeRequestDto request)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            var decision = await _manager.AssessIdeaNudgeAsync(
                workspaceId,
                projectId,
                topicId,
                request.IdeaText,
                request.Conversation.Select(turn => new IdeaNudgeTurn
                {
                    Question = turn.Question,
                    Answer = turn.Answer,
                }).ToList());

            return Ok(new IdeaNudgeResponseDto
            {
                IsApproved = decision.IsApproved,
                Question = decision.Question,
            });
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IdeaDto>>> GetAllIdeasOfTopic(Slug workspaceId, Slug projectId, int topicId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

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
    public async Task<ActionResult<IEnumerable<IdeaDto>>> DiscoverIdeas(
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
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            var ideas = await _manager.GetIdeaDiscoverySuggestionsAsync(
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
    public async Task<ActionResult<IdeaDto>> GetIdeaById(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            var idea = _manager.GetIdeaById(workspaceId, projectId, topicId, ideaId);

            return Ok(IdeaDto.From(idea));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("{ideaId:int}/thread")]
    public async Task<ActionResult<IdeaThreadDto>> GetIdeaThread(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            var idea = _manager.GetIdeaByIdWithProjectAndResponses(workspaceId, projectId, topicId, ideaId);

            return Ok(IdeaThreadDto.From(idea));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet("{ideaId:int}/reactions")]
    public async Task<ActionResult<IEnumerable<ReactionDto>>> GetIdeaReactionSummary(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

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
    public async Task<ActionResult<CreatedReactionDto>> AddIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

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
    public async Task<ActionResult> RemoveIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromQuery] Guid youthId, int reactionId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            _manager.RemoveIdeaReaction(workspaceId, projectId, topicId, ideaId, youthId, reactionId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPut("{ideaId:int}")]
    public async Task<ActionResult<IdeaDto>> UpdateAfterSafetyReview(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromBody] UpdateIdeaAfterSafetyReviewDto request)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            ModerationStatus newStatus = request.MarkForReview ? ModerationStatus.Pending : ModerationStatus.Approved;
            var updated = _manager.ChangeIdea(workspaceId, projectId, topicId, ideaId, newStatus, request.Content);
            
            return StatusCode(201, IdeaDto.From(updated));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    private async Task<bool> IsActiveProjectOrAdmin(Slug workspaceId, Slug projectId)
    {
        return await _projectAccessService.IsActiveProjectOrAdminAsync(workspaceId, projectId, User);
    }
}
