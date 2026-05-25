using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers;

[Route("{projectId}")]
public class ProjectController : Controller
{
    private readonly WorkspaceContext _workspaceContext;
    private readonly IProjectAccessService _projectAccessService;

    public ProjectController(WorkspaceContext workspaceContext, IProjectAccessService projectAccessService)
    {
        _workspaceContext = workspaceContext;
        _projectAccessService = projectAccessService;
    }

    [HttpGet]
    public async Task<IActionResult> Survey(Slug projectId)
    {
        if (!await IsActiveProjectOrAdmin(projectId))
        {
            return NotFound();
        }

        return View();
    }
    
    [HttpGet("completed")]
    public async Task<IActionResult> Completed(Slug projectId)
    {
        if (!await IsActiveProjectOrAdmin(projectId))
        {
            return NotFound();
        }

        return View();
    }
    
    [HttpGet("ideas")]
    public async Task<IActionResult> Ideas(Slug projectId)
    {
        if (!await IsActiveProjectOrAdmin(projectId))
        {
            return NotFound();
        }

        return View();
    }

    private async Task<bool> IsActiveProjectOrAdmin(Slug projectId)
    {
        if (_workspaceContext.CurrentWorkspace == null)
        {
            return false;
        }

        return await _projectAccessService.IsActiveProjectOrAdminAsync(
            _workspaceContext.CurrentWorkspace.Id,
            projectId,
            User);
    }
}
