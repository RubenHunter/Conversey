using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;
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

    public IdeasController(IIdeaManager manager)
    {
        _manager = manager;
    }

    [HttpPost]
    public ActionResult<SubmissionResponseDto> Submit(Slug workspaceId, Slug projectId, int topicId, [FromBody] IdeaDto idea)
    {
        try
        {
            SubmissionResponse response = _manager.SubmitIdea(idea.Content, projectId, topicId, idea.YouthId);
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
    public ActionResult<IEnumerable<IdeaDto>> GetAllIdeasOfTopic(Slug workspaceId, Slug projectId, int topicId)
    {
        try
        {
            IEnumerable<Idea> ideas = _manager.GetIdeasByProjectIdAndTopicId(projectId, topicId);
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
    public ActionResult<IdeaDto> GetIdeaById(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        try
        {
            var idea = _manager.GetIdeaById(workspaceId, projectId, topicId, ideaId);

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
    public ActionResult<IdeaThreadDto> GetIdeaThread(Slug workspaceId, Slug projectId, int topicId, int ideaId)
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
    public ActionResult<IEnumerable<ReactionDto>> GetIdeaReactionSummary(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        try
        {
            return Ok(_manager.GetIdeaReactionsByIdeaId(workspaceId, projectId, topicId, ideaId).Select(ReactionDto.From));
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
    public ActionResult<ReactionDto> AddIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromBody] CreateResponseReactionRequestDto request)
    {
        try
        {
            IdeaReaction reaction = _manager.AddIdeaReaction(request.Emoji, ideaId, request.YouthToken.Trim());
            return Ok(ReactionDto.From(reaction));
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

    [HttpDelete("{ideaId:int}/reactions/{reactionId:int}")]
    public ActionResult RemoveIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, [FromQuery] Guid youthId, int reactionId)
    {
        try
        {
            _manager.RemoveIdeaReaction(workspaceId, projectId, topicId, ideaId, youthId, reactionId);
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
        Slug workspaceId,
        Slug projectId,
        int topicId,
        int ideaId,
        [FromBody] UpdateIdeaAfterSafetyReviewDto request)
    {
        try
        {
            Idea newIdea = new Idea
            {
                Id = ideaId,
                Status = request.MarkForReview ? ModerationStatus.Pending : ModerationStatus.Approved,
                Content = request.Content,
            };
            var updated = _manager.ChangeIdea(workspaceId, projectId, topicId, newIdea);

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
