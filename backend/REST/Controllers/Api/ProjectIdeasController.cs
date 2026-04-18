using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Ideation;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}/ideas")]
public class ProjectIdeasController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;

    public ProjectIdeasController(IIdeaManager ideaManager)
    {
        _ideaManager = ideaManager;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<IdeaDto>> GetIdeasByYouth(Slug workspaceId, Slug projectId, [FromQuery] Guid youthId)
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

    [HttpGet("by-youth/{youthId:guid}")]
    public ActionResult<IReadOnlyCollection<IdeaDto>> GetIdeasByYouthPath(Slug workspaceId, Slug projectId, Guid youthId)
    {
        return GetIdeasByYouth(workspaceId, projectId, youthId);
    }
}
