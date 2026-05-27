using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Conversey.BL.Administration;
using Conversey.BL.Ai;
using Conversey.BL.Analytics;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Services;
using Conversey.BL.Survey;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Survey;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Models.Admin;
using Conversey.UI_MVC.Models.AdminManagement;
using Conversey.UI_MVC.Models.WorkspaceAdmin;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Conversey.UI_MVC.Controllers.Admin;
[Authorize(Policy = WorkspaceAdminPolicy.Name)]
public class WorkspaceAdminController(Workspace currentWorkspace, IProjectManager projectManager, IQuestionManager questionManager, IAiAdminManager aiAdminManager, IAnalyticsManager analyticsManager, IAdminManager adminManager, IEmailService emailService) : Controller
{
    [HttpGet("/admin/workspace")]
    public IActionResult Index()
    {
        TempData.Keep("ForcePasswordChange");
        return Redirect("/admin");
    }

    [HttpGet("/admin/projects")]
    public IActionResult Projects()
    {
        var projects = projectManager.GetAllProjectsFromWorkspaceId(currentWorkspace.Id);
        var cardVms = projects.Select(p => new ProjectCardViewModel()
        {
            Id = p.Id,
            Title = p.Name,
            ImageUrl = p.ImageUrl,
            Status = p.Status
        }).ToList();

        var participation = analyticsManager.GetParticipationStats(currentWorkspace.Id, null);
        var platformStats = analyticsManager.GetPlatformStats(currentWorkspace.Id).FirstOrDefault();

        var vm = new ProjectsPageViewModel
        {
            Projects         = cardVms,
            TotalProjects    = cardVms.Count,
            ParticipantCount = participation.TotalYouth,
            IdeaCount        = platformStats?.IdeaCount ?? 0,
            AnswerCount      = platformStats?.AnswerCount ?? 0,
        };

        return View(vm);
    }

    [HttpGet("/admin/projects/new")]
    public async Task<IActionResult> CreateProject([FromQuery] string? copy)
    {
        if (string.IsNullOrWhiteSpace(copy))
        {
            var projectStep1 = new CreateProjectIntroAndPresentationViewModel();
            return View(await CreateFormVmAsync(projectStep1, null, false));
        }

        try
        {
            var sourceProject = projectManager.GetProjectById(currentWorkspace.Id, new Slug { Text = copy });
            var projectStep1Copy = new CreateProjectIntroAndPresentationViewModel
            {
                Name = sourceProject.Name,
                Description = sourceProject.Description,
                ImageUrl = sourceProject.ImageUrl,
                InteractionForm = sourceProject.InteractionForm,
                StartDate = sourceProject.StartDate.Date,
                EndDate = sourceProject.EndDate.Date,
                NudgingStrength = sourceProject.NudgingStrength,
                MinAge = sourceProject.MinAge,
                MaxAge = sourceProject.MaxAge,
                Status = Status.Draft,
                Slug = string.Empty,
                ThemePrimary = (sourceProject.Theme ?? ProjectTheme.Default).Primary,
                ThemeSecondary = (sourceProject.Theme ?? ProjectTheme.Default).Secondary,
                ThemeAccent = (sourceProject.Theme ?? ProjectTheme.Default).Accent,
                ThemePreset = (sourceProject.Theme ?? ProjectTheme.Default).Preset,
                ThemeFont = (sourceProject.Theme ?? ProjectTheme.Default).Font
            };

            return View(await CreateFormVmAsync(projectStep1Copy, null, true, sourceProject));
        }
        catch (NotFoundException notFoundException)
        {
            ModelState.AddModelError(string.Empty, notFoundException.Message);
            var projectStep1 = new CreateProjectIntroAndPresentationViewModel();
            return View(await CreateFormVmAsync(projectStep1, null, false));
        }
    }

    [HttpPost("/admin/projects/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProject(ProjectViewModel projectViewModel)
    {
        var projectStep1 = projectViewModel.CreateStep1ViewModel;

        if (!ModelState.IsValid)
        {
            return View(await CreateFormVmAsync(projectStep1));
        }

        try
        {
            if (ProjectExistsAsNonDraft(projectStep1))
            {
                ModelState.AddModelError("CreateStep1ViewModel.Name",
                    "Project name already exists. Draft can save, but creation blocked until name unique.");
                return View(await CreateFormVmAsync(projectStep1));
            }

            var imageUrl = await ResolveProjectImageUrl(projectStep1);
            var project = projectManager.SaveProject(
                currentWorkspace.Id,
                projectStep1.Name,
                projectStep1.Description,
                projectStep1.StartDate,
                projectStep1.EndDate,
                projectStep1.InteractionForm,
                imageUrl,
                projectStep1.NudgingStrength,
                projectStep1.MinAge,
                projectStep1.MaxAge,
                Status.Active,
                projectStep1.Slug,
                new ProjectTheme { Primary = projectStep1.ThemePrimary, Secondary = projectStep1.ThemeSecondary, Accent = projectStep1.ThemeAccent, Preset = projectStep1.ThemePreset, Font = projectStep1.ThemeFont }
            );

            var step2 = projectViewModel.CreateStep2ViewModel;
            SaveQuestionsFromStep2(step2, project.Id);

            var step3 = projectViewModel.CreateStep3ViewModel;
            SaveTopicsFromStep3(step3, project.Id);

            var step4 = projectViewModel.CreateStep4ViewModel;
            await SavePromptsFromStep4(step4, project.Id);

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

        return View(await CreateFormVmAsync(projectStep1));
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
            var project       = projectManager.GetProjectById(currentWorkspace.Id, id);
            var participation = analyticsManager.GetParticipationStats(currentWorkspace.Id, id);
            var vm = new ProjectDetailsViewModel
            {
                Project          = project,
                Questions        = questionManager.GetQuestions(currentWorkspace.Id, id),
                ParticipantCount = participation.TotalYouth,
                IdeaCount        = analyticsManager.GetIdeaStats(currentWorkspace.Id, id, null).Count,
            };
            return View(vm);
        }
        catch (NotFoundException e)
        {
            //TODO 404 Page
            Console.WriteLine(e);
            throw;
        }
    }

    [HttpGet("/admin/project/{id}")]
    public async Task<IActionResult> EditProject(Slug id)
    {
        try
        {
            var project = projectManager.GetProjectById(currentWorkspace.Id, id);

            return View(await CreateFormVmAsync(new CreateProjectIntroAndPresentationViewModel
            {
                Name = project.Name,
                Description = project.Description,
                ImageUrl = project.ImageUrl,
                InteractionForm = project.InteractionForm,
                StartDate = project.StartDate.Date,
                EndDate = project.EndDate.Date,
                NudgingStrength = project.NudgingStrength,
                MinAge = project.MinAge,
                MaxAge = project.MaxAge,
                Slug = project.Id.ToString(),
                Status = project.Status,
                ThemePrimary = (project.Theme ?? ProjectTheme.Default).Primary,
                ThemeSecondary = (project.Theme ?? ProjectTheme.Default).Secondary,
                ThemeAccent = (project.Theme ?? ProjectTheme.Default).Accent,
                ThemePreset = (project.Theme ?? ProjectTheme.Default).Preset,
                ThemeFont = (project.Theme ?? ProjectTheme.Default).Font
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
                return View(await CreateFormVmAsync(projectStep1, projectManager.GetProjectById(currentWorkspace.Id, id)));
            }

            var imageUrl = await ResolveProjectImageUrl(projectStep1);
            projectManager.SaveProject(
                currentWorkspace.Id,
                projectStep1.Name,
                projectStep1.Description,
                projectStep1.StartDate,
                projectStep1.EndDate,
                projectStep1.InteractionForm,
                imageUrl,
                projectStep1.NudgingStrength,
                projectStep1.MinAge,
                projectStep1.MaxAge,
                projectStep1.Status,
                id.ToString(),
                new ProjectTheme { Primary = projectStep1.ThemePrimary, Secondary = projectStep1.ThemeSecondary, Accent = projectStep1.ThemeAccent, Preset = projectStep1.ThemePreset, Font = projectStep1.ThemeFont }
            );

            var step2 = projectViewModel.CreateStep2ViewModel;
            SaveQuestionsFromStep2(step2, id);

            var step4 = projectViewModel.CreateStep4ViewModel;
            await SavePromptsFromStep4(step4, id);

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

        return View(await CreateFormVmAsync(projectViewModel.CreateStep1ViewModel, projectManager.GetProjectById(currentWorkspace.Id, id)));
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
                currentWorkspace.Id,
                projectStep1.Name,
                projectStep1.Description,
                projectStep1.StartDate,
                projectStep1.EndDate,
                projectStep1.InteractionForm,
                imageUrl,
                projectStep1.NudgingStrength,
                projectStep1.MinAge,
                projectStep1.MaxAge,
                Status.Draft,
                projectStep1.Slug,
                new ProjectTheme { Primary = projectStep1.ThemePrimary, Secondary = projectStep1.ThemeSecondary, Accent = projectStep1.ThemeAccent, Preset = projectStep1.ThemePreset, Font = projectStep1.ThemeFont }
            );

            var step2 = projectViewModel.CreateStep2ViewModel;
            SaveQuestionsFromStep2(step2, project.Id);

            var step3 = projectViewModel.CreateStep3ViewModel;
            SaveTopicsFromStep3(step3, project.Id);

            var step4 = projectViewModel.CreateStep4ViewModel;
            await SavePromptsFromStep4(step4, project.Id);

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
            projectManager.RemoveProject(id, currentWorkspace.Id);
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
            var project = projectManager.GetProjectById(currentWorkspace.Id, id);
            project.Status = Status.Archived;
            projectManager.EditProject(project);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    private void SaveQuestionsFromStep2(CreateStep2SurveyViewModel? step2, Slug projectId)
    {
        if (step2 == null || string.IsNullOrWhiteSpace(step2.QuestionsJson) || step2.QuestionsJson == "[]")
            return;

        List<QuestionDraftDto>? dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<QuestionDraftDto>>(step2.QuestionsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException) { return; }

        if (dtos == null || dtos.Count == 0) return;

        questionManager.RemoveQuestionsForProject(currentWorkspace.Id, projectId);

        var project = projectManager.GetProjectById(currentWorkspace.Id, projectId);

        foreach (var dto in dtos)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Text)) continue;

            Question question = dto.Type switch
            {
                "Scale" => new ScaleQuestion
                {
                    Text = dto.Text,
                    Required = dto.Required,
                    Project = project,
                    LowerBound = dto.Min ?? 1,
                    UpperBound = dto.Max ?? 5,
                },
                "MultipleChoice" => BuildChoiceQuestion<MultipleChoiceQuestion>(dto, project),
                "SingleChoice" => BuildChoiceQuestion<SingleChoiceQuestion>(dto, project),
                _ => new OpenQuestion { Text = dto.Text, Required = dto.Required, Project = project },
            };

            questionManager.AddQuestion(question);
        }
    }

    private static TQ BuildChoiceQuestion<TQ>(QuestionDraftDto dto, Project project)
        where TQ : ChoiceQuestion, new()
    {
        var q = new TQ { Text = dto.Text, Required = dto.Required, Project = project };
        q.PossibleChoices = dto.PossibleAnswers?
            .Where(a => !string.IsNullOrWhiteSpace(a.Text))
            .Select(a => new Choice { Text = a.Text, Question = q })
            .ToList() ?? new List<Choice>();
        return q;
    }

    private string SerializeQuestionsToJson(IEnumerable<Question> questions)
    {
        var dtos = questions.Select<Question, object?>(q => q switch
        {
            SingleChoiceQuestion scq => new
            {
                type = "SingleChoice",
                text = scq.Text,
                required = scq.Required,
                possibleAnswers = scq.PossibleChoices?.Select(c => new { text = c.Text }).ToList() ?? new(),
            },
            MultipleChoiceQuestion mcq => new
            {
                type = "MultipleChoice",
                text = mcq.Text,
                required = mcq.Required,
                possibleAnswers = mcq.PossibleChoices?.Select(c => new { text = c.Text }).ToList() ?? new(),
            },
            ScaleQuestion sq => new
            {
                type = "Scale",
                text = sq.Text,
                required = sq.Required,
                min = sq.LowerBound,
                max = sq.UpperBound,
            },
            OpenQuestion oq => new
            {
                type = "Open",
                text = oq.Text,
                required = oq.Required,
            },
            _ => null,
        }).Where(d => d != null).ToList();

        return JsonSerializer.Serialize(dtos);
    }

    private record QuestionDraftDto(
        string Type,
        string Text,
        bool Required,
        List<AnswerDraftDto>? PossibleAnswers,
        int? Min,
        int? Max);

    private record AnswerDraftDto(string Text);

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
                    currentWorkspace.Id,
                    topic.TopicName,
                    topic.TopicContext
                );
            }
        }
    }

    private async Task<ProjectViewModel> CreateFormVmAsync(CreateProjectIntroAndPresentationViewModel projectStep1, Project project = null, bool isCopy = false, Project copyFromProject = null)
    {
        var isCreatePage = project == null;
        var dataSource = project ?? copyFromProject;

        var step2 = new CreateStep2SurveyViewModel();
        var questionCount = 0;
        if (dataSource != null)
        {
            var existingQuestions = questionManager.GetQuestions(currentWorkspace.Id, dataSource.Id);
            questionCount = existingQuestions.Count();
            if (existingQuestions.Any())
                step2.QuestionsJson = SerializeQuestionsToJson(existingQuestions);
        }

        var step3 = new CreateStep3IdeationViewModel();
        var topicCount = 0;
        if (dataSource?.Topic != null && dataSource.Topic.Any())
        {
            topicCount = dataSource.Topic.Count();
            var topicRows = dataSource.Topic.Select(t => new TopicRowViewModel
            {
                TopicName = t.Name,
                TopicContext = t.Context ?? string.Empty,
                MaxBroadSelectionLoads = t.MaxBroadSelectionLoads,
            }).ToList();
            step3.TopicsJson = JsonSerializer.Serialize(topicRows, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        var allPrompts = await aiAdminManager.GetAllPromptsAsync();
        var step4Prompts = allPrompts
            .Where(p => !p.Name.EndsWith("System", StringComparison.OrdinalIgnoreCase)
                        && !p.Name.StartsWith("Moderation", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var step4 = new CreateStep4AiConfigViewModel();
        if (dataSource != null)
        {
            var existingOverrides = await aiAdminManager.GetProjectPromptOverridesAsync(dataSource.Id.ToString());
            if (existingOverrides.Any())
            {
                var overrideDtos = existingOverrides.Select(o => new { promptName = o.PromptName, userPromptTemplate = o.UserPromptTemplate });
                step4.PromptsJson = JsonSerializer.Serialize(overrideDtos, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
        }

        var participantCount = 0;
        var ideaCount = 0;
        if (dataSource != null)
        {
            participantCount = analyticsManager.GetParticipationStats(currentWorkspace.Id, dataSource.Id).TotalYouth;
            ideaCount = analyticsManager.GetIdeaStats(currentWorkspace.Id, dataSource.Id, null).Count;
        }

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
                    Status = projectStep1.Status,
                    MinAge = projectStep1.MinAge,
                    MaxAge = projectStep1.MaxAge
                },
                FormAction = project == null ? "CreateProject" : "EditProject",
                SubmitLabel = project == null ? "Create Project" : "Update Project",
            },
            CreateStep1ViewModel = projectStep1,
            CreateStep2ViewModel = step2,
            CreateStep3ViewModel = step3,
            CreateStep4ViewModel = step4,
            Step4Prompts = step4Prompts,
            ParticipantCount = participantCount,
            IdeaCount = ideaCount,
            QuestionCount = questionCount,
            TopicCount = topicCount,
            StepperViewModel = new StepperViewModel
            {
                Title = project == null ? "Creating a Project" : "Editing a Project",
                EntityName = "Project",
                DraftStoragePrefix = isCreatePage
                    ? $"workspace:{currentWorkspace.Id}:project-create"
                    : $"workspace:{currentWorkspace.Id}:project-edit:{project?.Id}",
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
                        PartialViewName = "_ProjectStepAiConfigForm"
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

    private async Task SavePromptsFromStep4(CreateStep4AiConfigViewModel step4, Slug projectId)
    {
        if (step4 == null || string.IsNullOrWhiteSpace(step4.PromptsJson) || step4.PromptsJson == "[]")
            return;

        List<PromptOverrideDto> dtos;
        try
        {
            dtos = JsonSerializer.Deserialize<List<PromptOverrideDto>>(step4.PromptsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException) { return; }

        if (dtos == null || dtos.Count == 0) return;

        var overrides = dtos
            .Where(d => !string.IsNullOrWhiteSpace(d.PromptName) && !string.IsNullOrWhiteSpace(d.UserPromptTemplate))
            .Select(d => new ProjectAiPromptOverride
            {
                PromptName = d.PromptName,
                UserPromptTemplate = d.UserPromptTemplate
            })
            .ToList();

        await aiAdminManager.SaveProjectPromptOverridesAsync(projectId.ToString(), overrides);
    }

    private record PromptOverrideDto(string PromptName, string UserPromptTemplate);
    
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

    [HttpGet("/admin/projects/preview")]
    public IActionResult Preview(string prefix)
    {
        return View("Preview", prefix);
    }

    [HttpGet("/admin/workspace/admins")]
    public IActionResult Admins()
    {
        var workspace = currentWorkspace;
        var admins = adminManager.GetAllWorkspaceAdminsByWorkspaceIdWithWorkspace(workspace.Id);
        var model = new WorkspaceAdminManagementViewModel
        {
            Admins = admins.ToList(),
            WorkspaceName = workspace.Name,
            WorkspaceId = workspace.Id.Text
        };
        return View("~/Views/WorkspaceAdmin/AdminManagement.cshtml", model);
    }

    [HttpPost("/admin/workspace/admins/create")]
    public async Task<IActionResult> CreateWorkspaceAdmin(
        [FromForm(Name = "FormItem.Email")] string email,
        [FromForm(Name = "FormItem.Username")] string username,
        [FromForm(Name = "FormItem.PhoneNumber")] string phoneNumber)
    {
        var workspace = currentWorkspace;
        try
        {
            var (admin, oneTimePassword) = await adminManager.AddWorkspaceAdmin(email, username, phoneNumber, workspace.Id);
            TempData["OneTimePassword"] = oneTimePassword;
            TempData["OneTimePasswordAdminEmail"] = admin.Email;

            var loginUrl = $"{Request.Scheme}://{workspace.Id.Text}.{Request.Host}/login";
            var emailBody = $@"Welcome to Conversey!

You have been added as a workspace admin for the workspace '{workspace.Id.Text}'.

Login: {loginUrl}
Email: {admin.Email}
Temporary password: {oneTimePassword}

You will be asked to change your password on first login.

This is an automated message. Please do not reply.";

            var emailSent = false;
            try
            {
                await emailService.SendEmailAsync(admin.Email, $"Your Conversey workspace admin account for {workspace.Id.Text}", emailBody);
                emailSent = true;
            }
            catch
            {
                // Email sending failure should not block admin creation
            }

            TempData["EmailSent"] = emailSent;

            if (IsAjax())
                return Ok(new { redirectUrl = Url.Action("Admins"), emailSent });
            return RedirectToAction("Admins");
        }
        catch (ValidationException ex)
        {
            ModelStateHelper.ApplyValidationException(ModelState, ex, "FormItem");
        }

        if (IsAjax())
            return BadRequest(new { errors = ToErrorMap(ModelState) });
        return RedirectToAction("Admins");
    }

    [HttpPost("/admin/workspace/admins/edit/{id}")]
    public async Task<IActionResult> EditWorkspaceAdmin(
        Guid id,
        [FromForm(Name = "FormItem.Email")] string email,
        [FromForm(Name = "FormItem.Username")] string username,
        [FromForm(Name = "FormItem.PhoneNumber")] string phoneNumber)
    {
        var workspace = currentWorkspace;
        var workspaceAdmin = new WorkspaceAdmin
        {
            Id = id,
            Email = email,
            Username = username,
            PhoneNumber = phoneNumber,
            Workspace = workspace
        };

        try
        {
            await adminManager.EditWorkspaceAdmin(workspaceAdmin);
            if (IsAjax())
                return Ok(new { redirectUrl = Url.Action("Admins") });
            return RedirectToAction("Admins");
        }
        catch (NotFoundException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (ValidationException ex)
        {
            ModelStateHelper.ApplyValidationException(ModelState, ex);
        }

        if (IsAjax())
            return BadRequest(new { errors = ToErrorMap(ModelState) });
        return RedirectToAction("Admins");
    }

    private bool IsAjax()
    {
        return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string[]> ToErrorMap(ModelStateDictionary modelState)
    {
        return modelState
            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors
                    .Select(err => err.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .ToArray());
    }

    private bool ProjectExistsAsNonDraft(CreateProjectIntroAndPresentationViewModel projectStep1)
    {
        if (string.IsNullOrWhiteSpace(projectStep1.Name)) return false;

        var slugText = !string.IsNullOrWhiteSpace(projectStep1.Slug)
            ? projectStep1.Slug
            : Slug.FromName(projectStep1.Name).ToString();

        try
        {
            var existing = projectManager.GetProjectById(currentWorkspace.Id, new Slug { Text = slugText });
            return existing.Status != Status.Draft;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }
}
