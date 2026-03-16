using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceSlug}/projects/{projectSlug}/ideas")]
public class ProjectIdeasController : ControllerBase
{
    private readonly IIdeaManager _ideaManager;
    private readonly IProjectManager _projectManager;

    public ProjectIdeasController(IIdeaManager ideaManager, IProjectManager projectManager)
    {
        _ideaManager = ideaManager;
        _projectManager = projectManager;
    }

    [HttpGet("by-youth/{youthToken}")]
    public ActionResult<IReadOnlyCollection<IdeaDto>> GetIdeasByYouth(string workspaceSlug, string projectSlug, string youthToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(youthToken))
            {
                return BadRequest("YouthToken is required.");
            }

            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            var ideas = _ideaManager.GetIdeasFromProjectByYouthToken(project.Id, youthToken.Trim())
                .Select(IdeaDto.From)
                .ToList()
                .AsReadOnly();

            return Ok(ideas);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
    }

    private Project GetProjectForWorkspace(string workspaceSlug, string projectSlug)
    {
        var project = _projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ToSlug(projectSlug));

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

