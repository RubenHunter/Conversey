using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Ideation;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}")]
public class ProjectIdeasController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectManager _projectManager;

    public ProjectIdeasController(IIdeaManager ideaManager, IProjectManager projectManager)
    {
        _ideaManager = ideaManager;
        _projectManager = projectManager;
    }

    [HttpGet("youth/{youthId:guid}/ideas")]
    public ActionResult<IReadOnlyCollection<IdeaDto>> GetIdeasByYouth(Slug workspaceId, Slug projectId, Guid youthId)
    {
        try
        {
            var ideas = _ideaManager.GetIdeasFromProjectByYouthId(workspaceId, projectId, youthId)
                .Select(IdeaDto.From)
                .ToList()
                .AsReadOnly();

            return Ok(ideas);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException)
        {
            return BadRequest();
        }
    }

    [HttpGet("ideas/by-youth/{youthId:guid}")]
    public ActionResult<IReadOnlyCollection<IdeaDto>> GetIdeasByYouthPath(Slug workspaceId, Slug projectId, Guid youthId)
    {
        return GetIdeasByYouth(workspaceId, projectId, youthId);
    }

    [HttpGet("ideas")]
    public ActionResult<IReadOnlyCollection<IdeaDto>> GetIdeasByYouthQuery(Slug workspaceId, Slug projectId, [FromQuery] Guid youthId)
    {
        return GetIdeasByYouth(workspaceId, projectId, youthId);
    }

    [HttpPut("youth/{youthId:guid}")]
    public ActionResult SaveYouthContactEmail(Slug workspaceId, Slug projectId, Guid youthId, [FromBody] Conversey.UI_MVC.Models.Dto.YouthContactEmailDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest("Email is required.");
        }

        try
        {
            _projectManager.AddYouth(youthId, dto.Email, projectId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }
}
