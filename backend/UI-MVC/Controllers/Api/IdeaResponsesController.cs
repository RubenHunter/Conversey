using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Ideation;
using Conversey.UI_MVC.Models.Dto;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/topics/{topicId:int}/ideas/{ideaId:int}/responses")]
public class IdeaResponsesController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectAccessService _projectAccessService;

    public IdeaResponsesController(IIdeaManager ideaManager, IProjectAccessService projectAccessService)
    {
        _ideaManager = ideaManager;
        _projectAccessService = projectAccessService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResponseDto>>> GetApprovedResponsesByYouth(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromQuery] Guid youthId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            var responses = _ideaManager.GetApprovedResponsesByYouth(workspaceId, projectId, topicId, ideaId, youthId)
                .Select(ResponseDto.From)
                .ToList()
                .AsReadOnly();

            return Ok(responses);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ResponseSubmissionResponseDto>> AddResponse(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromBody] CreateResponseRequestDto request)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            var submission = await _ideaManager.AddResponseAsync(workspaceId, projectId, topicId, ideaId, request.YouthId, request.Text);
            return Ok(submission switch
            {
                ResponseSubmissionResponse.Approved approved => new ResponseSubmissionResponseDto.Approved(ResponseDto.From(approved.IdeaResponse)),
                ResponseSubmissionResponse.Pending pending => new ResponseSubmissionResponseDto.Pending(ResponseDto.From(pending.IdeaResponse), pending.decision),
                _ => throw new InvalidOperationException("Unknown response submission type")
            });
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("{responseId:int}")]
    public async Task<ActionResult<ResponseDto>> UpdateAfterSafetyReview(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, [FromBody] UpdateResponseAfterSafetyReviewDto request)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            ModerationStatus newStatus = request.MarkForReview ? ModerationStatus.Pending : ModerationStatus.Approved;
            var updated = _ideaManager.ChangeResponse(workspaceId, projectId, topicId, ideaId, request.YouthId, responseId, newStatus, request.Text);
            return Ok(ResponseDto.From(updated));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{responseId:int}/reactions")]
    public async Task<ActionResult<IEnumerable<ReactionDto>>> GetReactionSummary(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            var reactions = _ideaManager.GetResponseReactionsByResponseId(workspaceId, projectId, topicId, ideaId, responseId)
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

    [HttpPost("{responseId:int}/reactions")]
    public async Task<ActionResult<CreatedReactionDto>> AddReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            ResponseReaction addedReaction = _ideaManager.AddResponseReaction(workspaceId, projectId, topicId, ideaId, responseId, request.YouthId, request.Emoji);

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
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("{responseId:int}/reactions/{reactionId:int}")]
    public async Task<ActionResult> RemoveReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, [FromQuery] Guid youthId, int reactionId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

            _ideaManager.RemoveResponseReaction(workspaceId, projectId, topicId, ideaId, responseId, youthId, reactionId);
            return NoContent();
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    private async Task<bool> IsActiveProjectOrAdmin(Slug workspaceId, Slug projectId)
    {
        return await _projectAccessService.IsActiveProjectOrAdminAsync(workspaceId, projectId, User);
    }

}
