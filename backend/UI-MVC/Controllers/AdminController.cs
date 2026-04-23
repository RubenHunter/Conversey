using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Conversey.UI_MVC.Security;

namespace Conversey.UI_MVC.Controllers;

[Authorize(Policy = WorkspaceAdminPolicy.Name)]
public class AdminController : Controller
{
    [HttpGet("/admin")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("/admin/projects")]
    public IActionResult Projects()
    {
        return View();
    }

    [HttpGet("/admin/projects/new")]
    public IActionResult CreateProject()
    {
        return View();
    }
}
