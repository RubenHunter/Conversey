using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [Route("/")]
    public IActionResult Index()
    {
        string host = Request.Host.Host;
        _logger.LogInformation("Request received from host: {Host}", host);
        
        // Als we op het hoofddomein zijn, ga naar login
        if (host.Equals("conversey.be", StringComparison.OrdinalIgnoreCase) || 
            host.Equals("www.conversey.be", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect("/login");
        }

        // Anders (subdomein), ga naar de landing pagina
        return RedirectToAction("Landing", "Project");
    }
}
