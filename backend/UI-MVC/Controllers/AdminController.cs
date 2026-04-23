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
}
