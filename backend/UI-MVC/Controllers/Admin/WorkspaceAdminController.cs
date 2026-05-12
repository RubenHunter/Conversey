using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Models.Admin;
using Conversey.UI_MVC.Models.WorkspaceAdmin;
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
        return View();
    }

    [HttpGet("/admin/projects")]
    public IActionResult Projects()
    {
        var projects = projectManager.GetAllProjectsFromWorkspaceId(workspaceContext.CurrentWorkspace.Id);
        return View(projects.Select(p => new ProjectCardViewModel()
        {
            Id = p.Id,
            Title = p.Name,
            ImageUrl = p.ImageUrl
        }).ToList());
    }

    [HttpGet("/admin/projects/new")]
    public IActionResult CreateProject()
    {
        return View(CreateFormVm(new CreateProjectStepOneViewModel()));
    }

    [HttpPost("/admin/projects/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProject(ProjectViewModel projectViewModel)
    {
        var projectStep1 = projectViewModel.CreateStep1ViewModel;

        if (!ModelState.IsValid)
        {
            return View(CreateFormVm(projectStep1));
        }

        try
        {
            var imageUrl = await ResolveProjectImageUrl(projectStep1);
            projectManager.AddProject(
                workspaceContext.CurrentWorkspace.Id,
                projectStep1.Name,
                projectStep1.Description,
                projectStep1.StartDate,
                projectStep1.EndDate,
                projectStep1.InteractionForm,
                imageUrl
            );

            return RedirectToAction("Projects");
        }
        catch (NotFoundException notFoundException)
        {
            //TODO 404
            ModelState.AddModelError(string.Empty, notFoundException.Message);
        }
        catch (ValidationException ex)
        {
            ApplyValidationExceptionToModelState(ex, "CreateStep1ViewModel");
        }

        return View(CreateFormVm(projectStep1));
    }

    [HttpPost("/admin/projects/new/upload-image")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCreateProjectImage(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest(new { error = "Please select an image file." });
        }

        if (string.IsNullOrWhiteSpace(imageFile.ContentType) ||
            !imageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only image files are allowed." });
        }

        await using var stream = imageFile.OpenReadStream();
        var imageUrl = await projectManager.UploadProjectImage(stream, imageFile.FileName, imageFile.ContentType);
        return Json(new { imageUrl });
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


    private ProjectViewModel CreateFormVm(CreateProjectStepOneViewModel projectStep1)
    {
        return new ProjectViewModel
        {
            AdminFormViewModel = new AdminFormViewModel<Project>
            {
                FormItem = new Project
                {
                    Name = projectStep1.Name,
                    Description = projectStep1.Description,
                    ImageUrl = projectStep1.ImageUrl,
                    InteractionForm = projectStep1.InteractionForm,
                    StartDate = projectStep1.StartDate,
                    EndDate = projectStep1.EndDate
                },
                FormAction = "CreateProject",
                SubmitLabel = "Create Project",
            },
            CreateStep1ViewModel = projectStep1,
            StepperViewModel = new StepperViewModel
            {
                Title = "Creating a Project",
                EntityName = "Project",
                DraftStoragePrefix = $"workspace:{workspaceContext.CurrentWorkspace.Id}:project-create",
                ImageUploadUrl = Url.Action(nameof(UploadCreateProjectImage)) ?? "/admin/projects/new/upload-image",
                Steps =
                [
                    new StepItem
                    {
                        Label = "Intro & Presentation",
                        PartialViewName = "_ProjectStep1Form"
                    },
                    new StepItem
                    {
                        Label = "Survey",
                        PartialViewName = "_ProjectStepPlaceholder"
                    },
                    new StepItem
                    {
                        Label = "Ideation",
                        PartialViewName = "_ProjectStepPlaceholder"
                    },
                    new StepItem
                    {
                        Label = "AI Configuration",
                        PartialViewName = "_ProjectStepPlaceholder"
                    },
                    new StepItem
                    {
                        Label = "Done",
                        PartialViewName = "_ProjectStepPlaceholder"
                    }
                ]
            }
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
    
    private async Task<string> ResolveProjectImageUrl(CreateProjectStepOneViewModel projectStep1)
    {
        if (projectStep1.ImageFile == null || projectStep1.ImageFile.Length == 0)
        {
            return projectStep1.ImageUrl ?? string.Empty;
        }

        await using var stream = projectStep1.ImageFile.OpenReadStream();
        return await projectManager.UploadProjectImage(stream, projectStep1.ImageFile.FileName, projectStep1.ImageFile.ContentType);
    }

    private void ApplyValidationExceptionToModelState(ValidationException ex, string memberPrefix = "Project")
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
                        ModelState.AddModelError($"{memberPrefix}.{member}", message);
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
