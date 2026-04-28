using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Conversey.UI_MVC.Security;

namespace Conversey.UI_MVC.Controllers;

[Authorize(Policy = WorkspaceAdminPolicy.Name)]
public class AdminController(WorkspaceContext workspaceContext, IProjectManager projectManager) : Controller
{
    [HttpGet("/admin")]
    public IActionResult Index()
    {
        return View();
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
        return View(CreateVm(new Project{StartDate = DateTime.Today, EndDate = DateTime.Today}));
    }

    [HttpPost("/admin/projects/new")]
    public IActionResult CreateProject(ProjectFormViewModel projectFormViewModel)
    {
        try
        {
            var project = projectFormViewModel.Project;
            projectManager.AddProject(workspaceContext.CurrentWorkspace.Id, project.Name, project.Description,
                project.Status, project.StartDate, project.EndDate, project.InteractionForm);

            TempData["Success"] = "Project created successfully.";
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

        return View(CreateVm(projectFormViewModel.Project));
    }

    [HttpGet("/admin/projects/{id}")]
    public IActionResult ProjectDetails(Slug id)
    {
        try
        {
            var project = projectManager.GetProjectById(workspaceContext.CurrentWorkspace.Id, id);
            return View(project);
        }
        catch (Exception e)
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
            
            return View(EditVm(project));
        }
        catch (NotFoundException e)
        {
            //TODO 404 Page
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpPost("/admin/project/{id}")]
    public IActionResult EditProject(Slug id, ProjectFormViewModel projectFormViewModel)
    {
        try
        {
            var project = projectFormViewModel.Project;
            project.Id = id;
            project.Workspace = workspaceContext.CurrentWorkspace;
            project.StartDate = project.StartDate.ToUniversalTime();
            project.EndDate = project.EndDate.ToUniversalTime();
            projectManager.EditProject(project);

            TempData["Success"] = "Project edited successfully.";
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

        return View(EditVm(projectFormViewModel.Project));
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


    private ProjectFormViewModel CreateVm(Project project)
    {
        return new ProjectFormViewModel
        {
            Project = project,
            FormAction = "CreateProject",
            SubmitLabel = "Create Project",
            IsEdit = false
        };
    }
    
    private ProjectFormViewModel EditVm(Project project)
    {
        return new ProjectFormViewModel
        {
            Project = project,
            FormAction = "EditProject",
            SubmitLabel = "Edit Project",
            IsEdit = false
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

