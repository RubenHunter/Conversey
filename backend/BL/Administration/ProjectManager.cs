using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;

namespace Conversey.BL.Administration;

public class ProjectManager: IProjectManager
{
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkspaceManager _workspaceManager;
    private readonly ICloudStorageRepository _cloudStorageRepository;

    public ProjectManager(IProjectRepository projectRepository, IWorkspaceManager workspaceManager, ICloudStorageRepository cloudStorageRepository)
    {
        _projectRepository = projectRepository;
        _workspaceManager = workspaceManager;
        _cloudStorageRepository = cloudStorageRepository;
    }

    public Project GetProject(Workspace workspace, Slug projectId)
    {
        return GetProjectById(workspace.Id, projectId);
    }

    public Project GetProjectById(Slug workspaceId, Slug projectId)
    {
        Project project = _projectRepository.ReadProjectByIdAndWorkspaceId(projectId, workspaceId);
        return project ?? throw new ProjectNotFoundException(projectId);
    }

    public Topic GetTopic(Project project, int topicId)
    {
        Topic topic = _projectRepository.ReadTopicByIdAndProjectId(topicId, project.Id);
        return topic ?? throw new TopicNotFoundException(topicId);
    }

    public Youth GetYouth(Project project, Guid youthId)
    {
        Youth youth = _projectRepository.ReadYouthByIdAndProjectId(youthId, project.Id);
        return youth ?? throw new YouthNotFoundException(youthId);
    }

    public Youth AddYouth(Guid token, string email, Slug projectId)
    {
        var project = _projectRepository.ReadProjectByIdWithWorkspaceAndTopicsAndYouthAndQuestions(projectId);
        if (project == null) throw new ProjectNotFoundException(projectId);

        var normalizedEmail = email?.Trim() ?? string.Empty;
        var existingYouth = _projectRepository.ReadYouthByIdAndProjectId(token, projectId);
        if (existingYouth != null)
        {
            if (YouthEmailHelper.ShouldReplaceEmail(existingYouth.Email, normalizedEmail))
            {
                existingYouth.Email = normalizedEmail;
                _projectRepository.UpdateYouth(existingYouth);
            }

            return existingYouth;
        }

        var youth = new Youth
        {
            Id = token,
            Email = normalizedEmail,
            Project = project
        };
        Validate(youth);
        _projectRepository.CreateYouth(youth);
        return youth;
    }

    public IEnumerable<Project> GetAllProjectsFromWorkspaceId(Slug workspaceId)
    {
        return _projectRepository.ReadAllProjectsFromWorkspaceId(workspaceId);
    }

    public Project AddProject(Slug workspaceId, string name, string description, DateTime startDate,
        DateTime endDate, InteractionType interactionForm, string imageUrl = "", int nudgingStrength = 3)
    {
        return SaveProject(
            workspaceId,
            name,
            description,
            startDate,
            endDate,
            interactionForm,
            imageUrl,
            nudgingStrength,
            null,
            null,
            Status.Active,
            null
        );
    }

    public Project SaveProject(Slug workspaceId, string name, string description, DateTime startDate,
        DateTime endDate, InteractionType interactionForm, string imageUrl, int nudgingStrength, int? minAge, int? maxAge,
        Status status, string slug, ProjectTheme theme = null)
    {
        var resolvedTheme = theme ?? ProjectTheme.Default;
        var workspace = _workspaceManager.GetWorkspaceById(workspaceId);

        if (string.IsNullOrWhiteSpace(name))
        {
            var results = new List<ValidationResult>
            {
                new("Name is required", ["Name"])
            };

            var ex = new ValidationException("Validation failed");
            ex.Data["ValidationResults"] = results;
            throw ex;
        }

        var resolvedSlug = string.IsNullOrWhiteSpace(slug) ? Slug.FromName(name) : Slug.FromName(slug);
        var existing = _projectRepository.ReadProjectByIdAndWorkspaceId(resolvedSlug, workspaceId);

        if (nudgingStrength < 1 || nudgingStrength > 5)
            throw new ValidationException("Nudging strength must be between 1 and 5.");

        if (existing == null)
        {
            var project = new Project
            {
                Id = resolvedSlug,
                Name = name,
                Description = description,
                ImageUrl = imageUrl?.Trim() ?? string.Empty,
                Status = status,
                StartDate = startDate.ToUniversalTime(),
                EndDate = endDate.ToUniversalTime(),
                InteractionForm = interactionForm,
                NudgingStrength = nudgingStrength,
                MinAge = minAge,
                MaxAge = maxAge,
                Workspace = workspace
            };

            Validate(project);
            _projectRepository.CreateProject(project);

            _projectRepository.CreateTheme(new ProjectTheme
            {
                ProjectId = project.Id,
                Primary = resolvedTheme.Primary,
                Secondary = resolvedTheme.Secondary,
                Accent = resolvedTheme.Accent,
                Preset = resolvedTheme.Preset,
                Font = resolvedTheme.Font
            });

            return project;
        }

        existing.Name = name;
        existing.Description = description;
        existing.ImageUrl = imageUrl?.Trim() ?? string.Empty;
        existing.Status = status;
        existing.StartDate = startDate.ToUniversalTime();
        existing.EndDate = endDate.ToUniversalTime();
        existing.InteractionForm = interactionForm;
        existing.NudgingStrength = Math.Clamp(nudgingStrength, 1, 5);
        existing.MinAge = minAge;
        existing.MaxAge = maxAge;
        existing.Workspace = workspace;

        Validate(existing);
        _projectRepository.UpdateProject(existing);

        if (existing.Theme == null)
        {
            _projectRepository.CreateTheme(new ProjectTheme
            {
                ProjectId = existing.Id,
                Primary = resolvedTheme.Primary,
                Secondary = resolvedTheme.Secondary,
                Accent = resolvedTheme.Accent,
                Preset = resolvedTheme.Preset,
                Font = resolvedTheme.Font
            });
        }
        else
        {
            existing.Theme.Primary = resolvedTheme.Primary;
            existing.Theme.Secondary = resolvedTheme.Secondary;
            existing.Theme.Accent = resolvedTheme.Accent;
            existing.Theme.Preset = resolvedTheme.Preset;
            existing.Theme.Font = resolvedTheme.Font;
            _projectRepository.UpdateTheme(existing.Theme);
        }

        return existing;
    }

    public void EditProject(Project updatedProject)
    {
        Validate(updatedProject);
        var existing = GetProjectById(updatedProject.Workspace.Id, updatedProject.Id);

        if (existing == null)
            throw new ProjectNotFoundException(updatedProject.Id);

        if (updatedProject.NudgingStrength < 1 || updatedProject.NudgingStrength > 5)
            throw new ValidationException("Nudging strength must be between 1 and 5.");

        existing.Name = updatedProject.Name;
        existing.Description = updatedProject.Description;
        existing.Status = updatedProject.Status;
        existing.StartDate = updatedProject.StartDate;
        existing.EndDate = updatedProject.EndDate;
        existing.InteractionForm = updatedProject.InteractionForm;
        existing.NudgingStrength = updatedProject.NudgingStrength;
        existing.MinAge = updatedProject.MinAge;
        existing.MaxAge = updatedProject.MaxAge;
        existing.Workspace = updatedProject.Workspace;
        
        
        _projectRepository.UpdateProject(existing);
    }

    public void RemoveProject(Slug projectId, Slug workspaceId)
    {
        _projectRepository.DeleteProject(projectId, workspaceId);
    }
    

    public async Task<string> UploadProjectImage(Stream stream, string fileName, string contentType)
    {
        return await _cloudStorageRepository.UploadFileAsync(stream, fileName, contentType);
    }

    public void AddTopic(Slug projectId, Slug workspaceId , string name, string context)
    {
        var workspace = _workspaceManager.GetWorkspaceById(workspaceId);
        if (workspace == null)
            throw new WorkspaceNotFoundException(workspaceId);
        var project = _projectRepository.ReadProjectByIdAndWorkspaceId(projectId, workspaceId);
        if (project == null)
            throw new ProjectNotFoundException(projectId);

        var topic = new Topic
        {
            Name = name,
            Context = context,
            Project = project
        };
        
        Validate(project);
        _projectRepository.CreateTopic(topic);
    }
    

    private static bool ShouldReplaceEmail(string currentEmail, string newEmail)
    {
        if (IsPlaceholderEmail(newEmail)) return false;

        var normalizedCurrent = currentEmail?.Trim() ?? string.Empty;
        if (normalizedCurrent.Length == 0) return true;

        return normalizedCurrent.EndsWith("@local.invalid", StringComparison.OrdinalIgnoreCase) ||
               !string.Equals(normalizedCurrent, newEmail, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlaceholderEmail(string email)
    {
        var normalized = email?.Trim() ?? string.Empty;
        return normalized.Length == 0 || normalized.EndsWith("@local.invalid", StringComparison.OrdinalIgnoreCase);
    }


    private void Validate(object obj)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(obj);

        if (!Validator.TryValidateObject(obj, context, validationResults, true))
        {
            var ex = new ValidationException("Validation failed");

            // attach structured data
            ex.Data["ValidationResults"] = validationResults;

            throw ex;
        }
    }
}
