using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceSlug}/projects")]
public class ProjectController : ControllerBase
{
    private readonly IProjectManager _manager;

    public ProjectController(IProjectManager manager)
    {
        _manager = manager;
    }

    [HttpGet("{projectSlug}")]
    public ActionResult<ProjectDto> GetBySlug(string workspaceSlug, string projectSlug)
    {
        try
        {
            var project = ResolveProjectForWorkspace(_manager, workspaceSlug, projectSlug);
            return Ok(ProjectDto.From(project));
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
    }

    internal static Project ResolveProjectForWorkspace(IProjectManager manager, string workspaceSlug, string projectSlug)
    {
        var project = manager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ToSlug(projectSlug));

        if (!string.Equals(project.Workspace.Id.Text, workspaceSlug, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProjectNotFoundException($"{workspaceSlug}/{projectSlug}");
        }

        return project;
    }

    internal static Slug ToSlug(string value)
    {
        return Slug.FromName(value);
    }
}
