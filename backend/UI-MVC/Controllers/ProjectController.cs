using Conversey.BL.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers;

[Route("{projectId:regex(^(?!login|identity|admin|logout|health|api).*$)}")]
public class ProjectController : Controller
{
    [HttpGet]
    public IActionResult Survey(Slug projectId)
    {
        return View();
    }
    
    [HttpGet("completed")]
    public IActionResult Completed(Slug projectId)
    {
        return View();
    }
    
    [HttpGet("ideas")]
    public IActionResult Ideas(Slug projectId)
    {
        return View();
    }
}