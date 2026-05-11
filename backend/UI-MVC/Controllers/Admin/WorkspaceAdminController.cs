using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Models.Admin;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;
[Authorize(Policy = WorkspaceAdminPolicy.Name)]
public class WorkspaceAdminController(WorkspaceContext workspaceContext, IProjectManager projectManager) : Controller
{
    [HttpGet("/admin/workspace")]
    public IActionResult Index()
    {
        try 
        {
            return View();
        }
        catch (Exception ex)
        {
            return Content($"DIAGNOSTIC ERROR: {ex.Message}\n\nSTACK TRACE: {ex.StackTrace}", "text/plain");
        }
    }
    
    [HttpGet("/admin/diag")]
    public IActionResult Diag()
    {
        var dbStatus = "Unknown";
        try { dbStatus = projectManager.GetAllProjectsFromWorkspaceId(workspaceContext.CurrentWorkspace.Id).Count().ToString() + " projects found"; } catch (Exception e) { dbStatus = "Error: " + e.Message; }
        
        return Content($"Workspace: {workspaceContext.CurrentWorkspace?.Name ?? "NULL"}\nDB Status: {dbStatus}", "text/plain");
    }

    [HttpGet("/admin/projects")]
    public IActionResult Projects()
    {
        var projects = projectManager.GetAllProjectsFromWorkspaceId(workspaceContext.CurrentWorkspace.Id);
        return View(projects);
    }

    [HttpGet("/admin/projects/new")]
    public IActionResult CreateProject()
    {
        return View(CreateFormVm(new Project{StartDate = DateTime.Today, EndDate = DateTime.Today}));
    }

    [HttpPost("/admin/projects/new")]
    public IActionResult CreateProject(AdminFormViewModel<Project> projectFormViewModel)
    {
        try
        {
            var project = projectFormViewModel.FormItem;
            projectManager.AddProject(workspaceContext.CurrentWorkspace.Id, project.Name, project.Description,
                project.Status, project.StartDate, project.EndDate, project.InteractionForm, project.NudgingStrength);

            return RedirectToAction("Projects");
        }
        catch (NotFoundException notFoundException)
        {
            //TODO 404
            ModelState.AddModelError(string.Empty, notFoundException.Message);
        }
        catch (ValidationException ex)
        {
            ApplyValidationExceptionToModelState(ex);
        }

        return View(CreateFormVm(projectFormViewModel.FormItem));
    }

    [HttpGet("/admin/projects/{id}")]
    public IActionResult ProjectDetails(Slug id)
    {
        try
        {
            var project = projectManager.GetProjectById(workspaceContext.CurrentWorkspace.Id, id);
            return View(project);
        }
        catch (NotFoundException e)
        {
            //TODO 404 Page
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("/admin/project/{id}")]
    public IActionResult EditProject(Slug id)
    {
        try
        {
            var project = projectManager.GetProjectById(workspaceContext.CurrentWorkspace.Id, id);
            
            return View(EditFormVm(project));
        }
        catch (NotFoundException e)
        {
            //TODO 404 Page
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpPost("/admin/project/{id}")]
    public IActionResult EditProject(Slug id, AdminFormViewModel<Project> projectFormViewModel)
    {
        try
        {
            var project = projectFormViewModel.FormItem;
            project.Id = id;
            project.Workspace = workspaceContext.CurrentWorkspace;
            project.StartDate = project.StartDate.ToUniversalTime();
            project.EndDate = project.EndDate.ToUniversalTime();
            projectManager.EditProject(project);

            return RedirectToAction("Projects");
        }
        catch (NotFoundException notFoundException)
        {
            ModelState.AddModelError(string.Empty, notFoundException.Message);
        }
        catch (ValidationException ex)
        {
            ApplyValidationExceptionToModelState(ex);
        }

        return View(EditFormVm(projectFormViewModel.FormItem));
    }

    [HttpPost("/admin/project/delete/{id}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteProject(Slug id)
    {
        try
        {
            projectManager.RemoveProject(id, workspaceContext.CurrentWorkspace.Id);
            return RedirectToAction("Projects");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    private AdminFormViewModel<Project> CreateFormVm(Project project)
    {
        return new AdminFormViewModel<Project>
        {
            FormItem = project,
            FormAction = "CreateProject",
            SubmitLabel = "Create Project",
        };
    }
    
    private AdminFormViewModel<Project> EditFormVm(Project project)
    {
        return new AdminFormViewModel<Project>
        {
            FormItem = project,
            FormAction = "EditProject",
            SubmitLabel = "Edit Project",
        };
    }
    
    private void ApplyValidationExceptionToModelState(ValidationException ex)
    {
        if (ex.Data["ValidationResults"] is List<ValidationResult> results)
        {
            foreach (var result in results)
            {
                var message = result.ErrorMessage ?? "Invalid value";

                if (result.MemberNames.Any())
                {
                    foreach (var member in result.MemberNames)
                    {
                        ModelState.AddModelError($"Project.{member}", message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                }
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
    }
}
