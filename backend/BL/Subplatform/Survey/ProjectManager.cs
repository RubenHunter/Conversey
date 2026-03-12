using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Subplatform;
using Conversey.DAL.Subplatform.Survey;

namespace Conversey.BL.Subplatform.Survey;

public class ProjectManager: IProjectManager
{
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkspaceManager _workspaceManager;

    public ProjectManager(IProjectRepository projectRepository, IWorkspaceManager workspaceManager)
    {
        _projectRepository = projectRepository;
        _workspaceManager = workspaceManager;
    }

    public Project GetProjectById(int projectId)
    {
        return _projectRepository.ReadProjectById(projectId);
    }

    public IReadOnlyCollection<Project> GetAllProjects()
    {
        return _projectRepository.ReadAllProjects();
    }

    public IReadOnlyCollection<Project> GetProjectsByWorkspaceId(int workspaceId)
    {
        return _projectRepository.ReadProjectsByWorkspaceId(workspaceId);
    }

    public Project AddProject(string title, string description, Status status, DateTime startDate, DateTime endDate, InteractionType interactionForm, int workspaceId)
    {
        var workspace = _workspaceManager.GetWorkspaceById(workspaceId);
        var project = new Project
        {
            Title = title,
            Description = description,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            InteractionForm = interactionForm,
            Workspace = workspace,
            Topic = new List<Topic>(),
            Questions = new List<Question>(),
            Youths = new List<Youth>()
        };
        Validate(project);
        _projectRepository.CreateProject(project);
        return project;
    }

    public Project EditProject(Project project)
    {
        Validate(project);
        _projectRepository.UpdateProject(project);
        return project;
    }

    public void RemoveProject(int projectId)
    {
        _projectRepository.DeleteProject(projectId);
    }

    public Topic GetTopicById(int topicId)
    {
        return _projectRepository.ReadTopicById(topicId);
    }

    public IReadOnlyCollection<Topic> GetTopicsByProjectId(int projectId)
    {
        return _projectRepository.ReadTopicsByProjectId(projectId);
    }

    public Topic AddTopic(string name, string context, int projectId)
    {
        var project = _projectRepository.ReadProjectById(projectId);
        var topic = new Topic
        {
            Name = name,
            Context = context,
            Project = project
        };
        Validate(topic);
        _projectRepository.CreateTopic(topic);
        return topic;
    }

    public Topic EditTopic(Topic topic)
    {
        Validate(topic);
        _projectRepository.UpdateTopic(topic);
        return topic;
    }

    public void RemoveTopic(int topicId)
    {
        _projectRepository.DeleteTopic(topicId);
    }

    public Youth GetYouthByToken(string token)
    {
        return _projectRepository.ReadYouthByToken(token);
    }

    public IReadOnlyCollection<Youth> GetYouthsByProjectId(int projectId)
    {
        return _projectRepository.ReadYouthsByProjectId(projectId);
    }

    public Youth AddYouth(string token, string email, int projectId)
    {
        var project = _projectRepository.ReadProjectById(projectId);
        var youth = new Youth
        {
            Token = token,
            Email = email,
            Project = project
        };
        Validate(youth);
        _projectRepository.CreateYouth(youth);
        return youth;
    }

    public Youth EditYouth(Youth youth)
    {
        Validate(youth);
        _projectRepository.UpdateYouth(youth);
        return youth;
    }

    public void RemoveYouth(string token)
    {
        _projectRepository.DeleteYouth(token);
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
