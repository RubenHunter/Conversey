using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/Workspaces")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceManager _manager;

    public WorkspacesController(IWorkspaceManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public ActionResult<IEnumerable<WorkspaceDto>> Get()
    {
        var workspaces = _manager.GetAllWorkspaces().ToList();

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

    [HttpPost]
    public IActionResult Create([FromBody] WorkspaceDto dto)
    {
        try
        {
            var workspace = _manager.AddWorkspace(dto.Name);
    
            return CreatedAtAction(
                nameof(Create),
                new { slug = workspace.Id.Text },
                workspace);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Slug id)
    {
        try
        {
            var workspace = _manager.GetWorkspaceById(id);
            return Ok(WorkspaceDto.From(workspace));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}
