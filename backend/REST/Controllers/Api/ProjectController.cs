using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey;
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
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            return Ok(ProjectDto.From(project));
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
    }

    private Project GetProjectForWorkspace(string workspaceSlug, string projectSlug)
    {
        var project = _manager.GetProjectBySlugWithWorkspaceAndQuestions(ToSlug(projectSlug));

        if (!string.Equals(project.Workspace.Slug.Text, workspaceSlug, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProjectNotFoundException($"{workspaceSlug}/{projectSlug}");
        }

        return project;
    }

    private static Slug ToSlug(string value)
    {
        return new Slug { Text = value.Trim().ToLowerInvariant() };
    }
}
