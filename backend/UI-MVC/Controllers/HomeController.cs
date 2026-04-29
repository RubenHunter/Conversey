using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers;

public class HomeController(WorkspaceContext workspaceContext) : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        var workspace = workspaceContext.CurrentWorkspace;
        if (workspace == null)
        {
            return NotFound("Workspace not found.");
        }

        var firstProject = workspace.Projects?.FirstOrDefault();
        if (firstProject == null)
        {
            return NotFound($"No projects found for workspace: {workspace.Name}");
        }

        return RedirectToAction("Survey", "Project", new { projectId = firstProject.Id.Text });
    }
}
