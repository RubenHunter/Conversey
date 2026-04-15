using Conversey.BL.Administration;
using Conversey.BL.Ideation;
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
            if (!Guid.TryParse(youthToken?.Trim(), out var parsedToken))
            {
                return BadRequest("YouthToken must be a valid GUID.");
            }

            var project = ProjectController.ResolveProjectForWorkspace(_projectManager, workspaceSlug, projectSlug);
            var ideas = _ideaManager.GetIdeasFromProjectByYouthToken(project.Id, parsedToken)
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
}
