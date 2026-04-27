using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;

namespace Conversey.BL.Administration;

public class ProjectManager: IProjectManager
{
    private readonly IProjectRepository _projectRepository;

    public ProjectManager(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
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
            if (ShouldReplaceEmail(existingYouth.Email, normalizedEmail))
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
            throw new ValidationException(string.Join("; ", validationResults.Select(r => r.ErrorMessage)));
        }
    }
}
