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

    public ProjectManager(IProjectRepository projectRepository, IWorkspaceManager workspaceManager)
    {
        _projectRepository = projectRepository;
        _workspaceManager = workspaceManager;
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

        var youth = new Youth
        {
            Id = token,
            Email = email,
            Project = project
        };
        Validate(youth);
        _projectRepository.CreateYouth(youth);
        return youth;
    }

    public IReadOnlyCollection<Project> GetAllProjectsFromWorkspaceId(Slug workspaceId)
    {
        return _projectRepository.ReadAllProjectsFromWorkspaceId(workspaceId);
    }

    public Project AddProject(Slug workspaceId, string name, string description, Status status, DateTime startDate,
        DateTime endDate, InteractionType interactionForm)
    {
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
        
        var project = new Project
        {
            Id = Slug.FromName(name),
            Name = name,
            Description = description,
            Status = status,
            StartDate = startDate.ToUniversalTime(),
            EndDate = endDate.ToUniversalTime(),
            InteractionForm = interactionForm,

            Workspace = workspace
        };
        
        Validate(project);

        _projectRepository.CreateProject(project);
        return project;
    }

    public void EditProject(Project updatedProject)
    {
        Validate(updatedProject);
        var existing = GetProjectById(updatedProject.Workspace.Id, updatedProject.Id);

        if (existing == null)
            throw new ProjectNotFoundException(updatedProject.Id);

        existing.Name = updatedProject.Name;
        existing.Description = updatedProject.Description;
        existing.Status = updatedProject.Status;
        existing.StartDate = updatedProject.StartDate;
        existing.EndDate = updatedProject.EndDate;
        existing.InteractionForm = updatedProject.InteractionForm;
        existing.Workspace = updatedProject.Workspace;
        
        
        _projectRepository.UpdateProject(existing);
    }

    public void RemoveProject(Slug projectId, Slug workspaceId)
    {
        _projectRepository.DeleteProject(projectId, workspaceId);
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
