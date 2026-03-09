using Conversey.BL;
using Conversey.BL.Domain.Workspace;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/Workspaces")]
public class WorkspacesController : ControllerBase
{
    private readonly IManager _manager;
    
    public WorkspacesController(IManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<WorkspaceDto>> Get()
    {
        IReadOnlyCollection<Workspace> workspaces = _manager.GetAllWorkspaces();

        if (workspaces.Count == 0)
        {
            return NoContent();
        }
        
        IReadOnlyCollection<WorkspaceDto> dtos = workspaces
            .Select(WorkspaceDto.From)
            .ToList()
            .AsReadOnly();

        return Ok(dtos);
    }
}