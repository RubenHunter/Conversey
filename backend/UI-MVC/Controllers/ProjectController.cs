using Conversey.BL.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers;

[Route("{projectId}")]
public class ProjectController : Controller
{
    [HttpGet]
    [HttpGet("survey")]
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