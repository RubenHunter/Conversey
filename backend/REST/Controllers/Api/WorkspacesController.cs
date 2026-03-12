using Conversey.BL;
using Conversey.BL.Domain.Subplatform;
using Conversey.BL.Subplatform;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

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
    
    [HttpPost]
    public IActionResult Create([FromBody] WorkspaceDto dto)
    {
        try
        {
            var workspace = _manager.CreateWorkspace(dto.Name, dto.Slug);

            return CreatedAtAction(
                nameof(GetBySlug),
                new { slug = workspace.Slug },
                workspace);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpGet("{slug}")]
    public IActionResult GetBySlug(string slug)
    {
        try
        {
            var workspace = _manager.GetWorkspaceBySlug(slug);
            return Ok(WorkspaceDto.From(workspace));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("id/{id:int}")]
    public IActionResult GetById(int id)
    {
        try
        {
            var workspace = _manager.GetWorkspaceById(id);
            return Ok(WorkspaceDto.From(workspace));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}