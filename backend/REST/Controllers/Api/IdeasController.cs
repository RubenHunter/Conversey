using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/ideas")]
public class IdeasController : ControllerBase
{
    private readonly IIdeaManager _manager;

    public IdeasController(IIdeaManager manager)
    {
        _manager = manager;
    }

    [HttpPost]
    public ActionResult<SubmissionResponseDto> Submit(IdeaDto idea)
    {
        try
        {
            SubmissionResponse response = _manager.SubmitIdea(idea.Content, idea.ProjectId, idea.TopicId);
            return Ok(response switch
            {
                SubmissionResponse.Approved approved => new SubmissionResponseDto.Approved(new IdeaDto
                {
                    ProjectId = approved.idea.Project.Id,
                    Content = approved.idea.Content,
                }),
                SubmissionResponse.Pending pending => new SubmissionResponseDto.Pending(new IdeaDto
                {
                    ProjectId = pending.idea.Project.Id,
                    Content = pending.idea.Content,
                }
                    , pending.suggestion),
            });
        }
        catch (ProjectNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<IdeaDto>> GetAllIdeas()
    {
        IReadOnlyCollection<Idea> ideas = _manager.GetAllIdeas();
        return Ok(ideas.Select(i => new IdeaDto
        {
            ProjectId = i.Project.Id,
            Content = i.Content,
        }).ToList());
    }

    [HttpGet("{ideaId}")]
    public ActionResult<IdeaDto> GetIdeaById(int ideaId)
    {
        try
        {
            Idea idea = _manager.GetIdeaById(ideaId);
            return Ok(new IdeaDto
            {
                ProjectId = idea.Project.Id,
                Content = idea.Content,
            });
        }
        catch (IdeaNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}