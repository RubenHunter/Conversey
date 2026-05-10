using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers;

public class HomeController : Controller
{
    [Route("/")]
    public IActionResult Index()
    {
        string host = Request.Host.Host;
        
        // Als we op het hoofddomein zijn, ga naar login
        if (host.Equals("conversey.be", StringComparison.OrdinalIgnoreCase) || 
            host.Equals("www.conversey.be", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        // Anders (subdomein), ga naar de landing pagina
        return RedirectToAction("Landing", "Project");
    }
}
