using Microsoft.AspNetCore.Mvc;
using Conversey.DAL.Administration;
using Conversey.UI_MVC.Models;

namespace Conversey.UI_MVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProjectRepository _projectRepository;
    private readonly WorkspaceContext _workspaceContext;

    public HomeController(ILogger<HomeController> logger, IProjectRepository projectRepository, WorkspaceContext workspaceContext)
    {
        _logger = logger;
        _projectRepository = projectRepository;
        _workspaceContext = workspaceContext;
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

        // Anders (subdomein), ga naar het eerste project
        var workspace = _workspaceContext.CurrentWorkspace;
        if (workspace != null)
        {
            var projects = _projectRepository.ReadProjectsByWorkspace(workspace.Id);
            var firstProject = projects.FirstOrDefault();
            if (firstProject != null)
            {
                return Redirect($"/{firstProject.Id.Text}/Survey");
            }
        }

        return Content("Geen projecten gevonden voor deze workspace.");
    }
}
