using Conversey.BL.Services;
using Conversey.REST.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers;

[ApiController]
[Route("api/workspaces")]
public class WorkspaceController: ControllerBase
{ 
    private readonly WorkspaceService _workspaceService;
    
    public WorkspaceController(WorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateWorkspaceDto dto)
    {
        try
        {
            var workspace = _workspaceService.CreateWorkspace(dto.Name);

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

    [HttpGet]
    public IActionResult GetAllWorkspaces()
    {
        var workspaces = _workspaceService.GetWorkspaces();
        
        if (workspaces == null)
            return NotFound();
        
        return Ok(workspaces);
    }

    [HttpGet("{slug}")]
    public IActionResult GetBySlug(string slug)
    {
        var workspace = _workspaceService.GetBySlug(slug);

        if (workspace == null)
            return NotFound();

        return Ok(workspace);
    }
}

