using Conversey.BL.Administration;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Conversey.UI_MVC.Security;

namespace Conversey.UI_MVC.Controllers;

[Authorize(Policy = WorkspaceAdminPolicy.Name)]
public class AdminController(WorkspaceContext workspaceContext, IProjectManager projectManager) : Controller
{
    private readonly IProjectManager _projectManager = projectManager;

    [HttpGet("/admin")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("/admin/projects")]
    public IActionResult Projects()
    {
        var projects = _projectManager.GetAllProjectsFromWorkspaceId(workspaceContext.CurrentWorkspace.Id);
        return View(projects);
    }

    [HttpGet("/admin/projects/new")]
    public IActionResult CreateProject()
    {
        return View();
    }
}
