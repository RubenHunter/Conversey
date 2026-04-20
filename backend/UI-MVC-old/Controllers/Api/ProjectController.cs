using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceId}/projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectManager _manager;

    public ProjectController(IProjectManager manager)
    {
        _manager = manager;
    }

    [HttpGet("{projectId}")]
    public ActionResult<ProjectDto> GetById(Slug workspaceId, Slug projectId)
    {
        try
        {
            Project project = _manager.GetProjectById(workspaceId, projectId);
            return Ok(ProjectDto.From(project));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
