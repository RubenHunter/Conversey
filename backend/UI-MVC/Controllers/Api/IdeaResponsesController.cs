using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Ideation;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/topics/{topicId:int}/ideas/{ideaId:int}/responses")]
public class IdeaResponsesController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;

    public IdeaResponsesController(IIdeaManager ideaManager)
    {
        _ideaManager = ideaManager;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ResponseDto>> GetApprovedResponsesByYouth(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromQuery] Guid youthId)
    {
        try
        {
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
    public ActionResult<ResponseDto> UpdateAfterSafetyReview(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, [FromBody] UpdateResponseAfterSafetyReviewDto request)
    {
        try
        {
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
    public ActionResult<IEnumerable<ReactionDto>> GetReactionSummary(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId)
    {
        try
        {
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
    public ActionResult<CreatedReactionDto> AddReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
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
    public ActionResult RemoveReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, [FromQuery] Guid youthId, int reactionId)
    {
        try
        {
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

}

