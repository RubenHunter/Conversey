using System.ComponentModel.DataAnnotations;
using System.Text.Json;
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
            ImageUrl = p.ImageUrl,
            Status = p.Status
        }).ToList());
    }

    [HttpGet("/admin/projects/new")]
    public IActionResult CreateProject([FromQuery] string? copy)
    {
        if (string.IsNullOrWhiteSpace(copy))
        {
            var projectStep1 = new CreateProjectIntroAndPresentationViewModel();
            return View(CreateFormVm(projectStep1, null, false));
        }

        try
        {
            var sourceProject = projectManager.GetProjectById(workspaceContext.CurrentWorkspace.Id, new Slug { Text = copy });
            var projectStep1Copy = new CreateProjectIntroAndPresentationViewModel
            {
                Name = sourceProject.Name,
                Description = sourceProject.Description,
                ImageUrl = sourceProject.ImageUrl,
                InteractionForm = sourceProject.InteractionForm,
                StartDate = sourceProject.StartDate.Date,
                EndDate = sourceProject.EndDate.Date,
                NudgingStrength = sourceProject.NudgingStrength,
                Status = Status.Draft,
                Slug = string.Empty
            };

            return View(CreateFormVm(projectStep1Copy, null, true));
        }
        catch (NotFoundException notFoundException)
        {
            ModelState.AddModelError(string.Empty, notFoundException.Message);
            var projectStep1 = new CreateProjectIntroAndPresentationViewModel();
            return View(CreateFormVm(projectStep1, null, false));
        }
    }
    
    [HttpGet("/admin/projects/new/questions")]
    public IActionResult AddQuestions()
    {
        return View("AddQuestions/AddQuestions");
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
            if (ProjectExistsAsNonDraft(projectStep1))
            {
                ModelState.AddModelError("CreateStep1ViewModel.Name",
                    "Project name already exists. Draft can save, but creation blocked until name unique.");
                return View(CreateFormVm(projectStep1));
            }

            var imageUrl = await ResolveProjectImageUrl(projectStep1);
            var project = projectManager.SaveProject(
                workspaceContext.CurrentWorkspace.Id,
                projectStep1.Name,
                projectStep1.Description,
                projectStep1.StartDate,
                projectStep1.EndDate,
                projectStep1.InteractionForm,
                imageUrl,
                projectStep1.NudgingStrength,
                Status.Active,
                projectStep1.Slug
            );

            var step3 = projectViewModel.CreateStep3ViewModel;
            SaveTopicsFromStep3(step3, project.Id);

            return RedirectToAction("Projects");
        }
        catch (NotFoundException notFoundException)
        {
            //TODO 404
            ModelState.AddModelError(string.Empty, notFoundException.Message);
        }
        catch (ValidationException ex)
        {
            ModelStateHelper.ApplyValidationException(ModelState, ex, "CreateStep1ViewModel");
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

            return View(CreateFormVm(new CreateProjectIntroAndPresentationViewModel
            {
                Name = project.Name,
                Description = project.Description,
                ImageUrl = project.ImageUrl,
                InteractionForm = project.InteractionForm,
                StartDate = project.StartDate.Date,
                EndDate = project.EndDate.Date,
                NudgingStrength = project.NudgingStrength,
                Slug = project.Id.ToString(),
                Status = project.Status
            }, project));
        }
        catch (NotFoundException e)
        {
            //TODO 404 Page
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpPost("/admin/project/{id}")]
    public async Task<IActionResult> EditProject(Slug id, ProjectViewModel projectViewModel)
    {
        try
        {
            var projectStep1 = projectViewModel.CreateStep1ViewModel;

            if (!ModelState.IsValid)
            {
                return View(CreateFormVm(projectStep1, projectManager.GetProjectById(workspaceContext.CurrentWorkspace.Id, id)));
            }

            var imageUrl = await ResolveProjectImageUrl(projectStep1);
            projectManager.SaveProject(
                workspaceContext.CurrentWorkspace.Id,
                projectStep1.Name,
                projectStep1.Description,
                projectStep1.StartDate,
                projectStep1.EndDate,
                projectStep1.InteractionForm,
                imageUrl,
                projectStep1.NudgingStrength,
                projectStep1.Status,
                id.ToString()
            );

            return RedirectToAction("Projects");
        }
        catch (NotFoundException notFoundException)
        {
            ModelState.AddModelError(string.Empty, notFoundException.Message);
        }
        catch (ValidationException ex)
        {
            ModelStateHelper.ApplyValidationException(ModelState, ex, "CreateStep1ViewModel");
        }

        return View(CreateFormVm(projectViewModel.CreateStep1ViewModel, projectManager.GetProjectById(workspaceContext.CurrentWorkspace.Id, id)));
    }

    [HttpPost("/admin/projects/draft")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(ProjectViewModel projectViewModel)
    {
        var projectStep1 = projectViewModel.CreateStep1ViewModel;

        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Draft validation failed." });
        }

        try
        {
            if (ProjectExistsAsNonDraft(projectStep1))
            {
                return Conflict(new { error = "Project name already exists. Draft can save, but creation blocked until name unique." });
            }

            var imageUrl = await ResolveProjectImageUrl(projectStep1);
            var project = projectManager.SaveProject(
                workspaceContext.CurrentWorkspace.Id,
                projectStep1.Name,
                projectStep1.Description,
                projectStep1.StartDate,
                projectStep1.EndDate,
                projectStep1.InteractionForm,
                imageUrl,
                projectStep1.NudgingStrength,
                Status.Draft,
                projectStep1.Slug
            );

            var step3 = projectViewModel.CreateStep3ViewModel;
            SaveTopicsFromStep3(step3, project.Id);

            return Json(new { slug = project.Id.ToString() });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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

    [HttpPost("/admin/projects/{id}/archive")]
    [ValidateAntiForgeryToken]
    public IActionResult ArchiveProject(Slug id)
    {
        try
        {
            var project = projectManager.GetProjectById(workspaceContext.CurrentWorkspace.Id, id);
            project.Status = Status.Archived;
            projectManager.EditProject(project);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    private void SaveTopicsFromStep3(CreateStep3IdeationViewModel? step3, Slug projectId)
    {
        if (step3 == null || string.IsNullOrWhiteSpace(step3.TopicsJson))
            return;

        List<TopicRowViewModel>? topics;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            topics = JsonSerializer.Deserialize<List<TopicRowViewModel>>(step3.TopicsJson, options);
        }
        catch (JsonException)
        {
            return;
        }

        if (topics == null) return;

        foreach (var topic in topics)
        {
            if (!string.IsNullOrWhiteSpace(topic.TopicName))
            {
                projectManager.AddTopic(
                    projectId,
                    workspaceContext.CurrentWorkspace.Id,
                    topic.TopicName,
                    topic.TopicContext
                );
            }
        }
    }

    private ProjectViewModel CreateFormVm(CreateProjectIntroAndPresentationViewModel projectStep1, Project project = null, bool isCopy = false)
    {
        var isCreatePage = project == null;
        return new ProjectViewModel
        {
            AdminFormViewModel = new AdminFormViewModel<Project>
            {
                FormItem = new Project
                {
                    Id = project?.Id ?? Slug.FromName(projectStep1.Name),
                    Name = projectStep1.Name,
                    Description = projectStep1.Description,
                    ImageUrl = projectStep1.ImageUrl,
                    InteractionForm = projectStep1.InteractionForm,
                    StartDate = projectStep1.StartDate,
                    EndDate = projectStep1.EndDate,
                    Status = projectStep1.Status
                },
                FormAction = project == null ? "CreateProject" : "EditProject",
                SubmitLabel = project == null ? "Create Project" : "Update Project",
            },
            CreateStep1ViewModel = projectStep1,
            StepperViewModel = new StepperViewModel
            {
                Title = project == null ? "Creating a Project" : "Editing a Project",
                EntityName = "Project",
                DraftStoragePrefix = isCreatePage
                    ? $"workspace:{workspaceContext.CurrentWorkspace.Id}:project-create"
                    : $"workspace:{workspaceContext.CurrentWorkspace.Id}:project-edit:{project?.Id}",
                ImageUploadUrl = Url.Action(nameof(UploadCreateProjectImage)) ?? "/admin/projects/new/upload-image",
                DraftSaveUrl = Url.Action(nameof(SaveDraft)) ?? "/admin/projects/draft",
                ProjectListUrl = Url.Action(nameof(Projects)) ?? "/admin/projects",
                IsCreatePage = isCreatePage,
                IsCopyFlow = isCopy,
                Steps =
                [
                    new StepItem
                    {
                        Label = "Intro & Presentation",
                        PartialViewName = "_ProjectStepIntroAndPresentationForm"
                    },
                    new StepItem
                    {
                        Label = "Survey",
                        PartialViewName = "_AddQuestions"
                    },
                    new StepItem
                    {
                        Label = "Ideation",
                        PartialViewName = "_ProjectStepIdeationForm"
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
    
    private async Task<string> ResolveProjectImageUrl(CreateProjectIntroAndPresentationViewModel projectStep1)
    {
        if (projectStep1.ImageFile == null || projectStep1.ImageFile.Length == 0)
        {
            return projectStep1.ImageUrl ?? string.Empty;
        }

        await using var stream = projectStep1.ImageFile.OpenReadStream();
        return await projectManager.UploadProjectImage(stream, projectStep1.ImageFile.FileName, projectStep1.ImageFile.ContentType);
    }

    private bool ProjectExistsAsNonDraft(CreateProjectIntroAndPresentationViewModel projectStep1)
    {
        if (string.IsNullOrWhiteSpace(projectStep1.Name)) return false;

        var slugText = !string.IsNullOrWhiteSpace(projectStep1.Slug)
            ? projectStep1.Slug
            : Slug.FromName(projectStep1.Name).ToString();

        try
        {
            var existing = projectManager.GetProjectById(workspaceContext.CurrentWorkspace.Id, new Slug { Text = slugText });
            return existing.Status != Status.Draft;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }
}
