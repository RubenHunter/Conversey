using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Administration;
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
            var workspace = _manager.CreateWorkspace(dto.Name, new Slug { Text = dto.Slug });

            return CreatedAtAction(
                nameof(GetBySlug),
                new { slug = workspace.Id.Text },
                workspace);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{slug}")]
    public IActionResult GetBySlug(Slug slug)
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

    [HttpGet("id/{id}")]
    public IActionResult GetById(string id)
    {
        try
        {
            var workspace = _manager.GetWorkspaceById(new Slug { Text = id.Trim().ToLowerInvariant() });
            return Ok(WorkspaceDto.From(workspace));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
