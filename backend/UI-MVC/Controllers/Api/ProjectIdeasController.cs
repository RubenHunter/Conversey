using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Ideation;
using Conversey.UI_MVC.Models.Dto;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects/{projectId}")]
public class ProjectIdeasController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectManager _projectManager;
    private readonly IProjectAccessService _projectAccessService;

    public ProjectIdeasController(IIdeaManager ideaManager, IProjectManager projectManager, IProjectAccessService projectAccessService)
    {
        _ideaManager = ideaManager;
        _projectManager = projectManager;
        _projectAccessService = projectAccessService;
    }

    [HttpGet("youth/{youthId:guid}/ideas")]
    public async Task<ActionResult<IReadOnlyCollection<IdeaDto>>> GetIdeasByYouth(Slug workspaceId, Slug projectId, Guid youthId)
    {
        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

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
    public async Task<ActionResult<IReadOnlyCollection<IdeaDto>>> GetIdeasByYouthPath(Slug workspaceId, Slug projectId, Guid youthId)
    {
        return await GetIdeasByYouth(workspaceId, projectId, youthId);
    }

    [HttpGet("ideas")]
    public async Task<ActionResult<IReadOnlyCollection<IdeaDto>>> GetIdeasByYouthQuery(Slug workspaceId, Slug projectId, [FromQuery] Guid youthId)
    {
        return await GetIdeasByYouth(workspaceId, projectId, youthId);
    }

    [HttpPut("youth/{youthId:guid}")]
    public async Task<ActionResult> SaveYouthContactEmail(Slug workspaceId, Slug projectId, Guid youthId, [FromBody] Conversey.UI_MVC.Models.Dto.YouthContactEmailDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest("Email is required.");
        }

        try
        {
            if (!await IsActiveProjectOrAdmin(workspaceId, projectId))
            {
                return NotFound();
            }

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

    private async Task<bool> IsActiveProjectOrAdmin(Slug workspaceId, Slug projectId)
    {
        return await _projectAccessService.IsActiveProjectOrAdminAsync(workspaceId, projectId, User);
    }
}
