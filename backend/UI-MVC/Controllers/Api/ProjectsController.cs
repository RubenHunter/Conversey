using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models.Dto;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectManager _manager;
    private readonly IProjectAccessService _projectAccessService;

    public ProjectsController(IProjectManager manager, IProjectAccessService projectAccessService)
    {
        _manager = manager;
        _projectAccessService = projectAccessService;
    }

    [HttpGet("{projectId}")]
    public async Task<ActionResult<ProjectDto>> GetById(Slug workspaceId, Slug projectId)
    {
        try
        {
            Project project = _manager.GetProjectById(workspaceId, projectId);
            if (!await _projectAccessService.IsActiveProjectOrAdminAsync(workspaceId, projectId, User))
            {
                return NotFound();
            }
            return Ok(ProjectDto.From(project));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

}
