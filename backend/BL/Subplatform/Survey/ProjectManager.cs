using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
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
        return _projectRepository.ReadProjectById(projectId) ?? throw new ProjectNotFoundException(projectId.ToString());
    }

    public Project GetProjectByIdWithTopics(int projectId)
    {
        return _projectRepository.ReadProjectByIdWithTopics(projectId) ?? throw new ProjectNotFoundException(projectId.ToString());
    }

    public Project GetProjectByIdWithQuestions(int projectId)
    {
        return _projectRepository.ReadProjectByIdWithQuestions(projectId) ?? throw new ProjectNotFoundException(projectId.ToString());
    }

    public Project GetProjectByIdWithTopicsAndQuestions(int projectId)
    {
        return _projectRepository.ReadProjectByIdWithTopicsAndQuestions(projectId) ?? throw new ProjectNotFoundException(projectId.ToString());
    }

    public Project GetProjectByIdWithWorkspaceAndQuestions(int projectId)
    {
        return _projectRepository.ReadProjectByIdWithWorkspaceAndQuestions(projectId) ?? throw new ProjectNotFoundException(projectId.ToString());
    }

    public Project GetProjectByIdWithWorkspaceTopicsYouthsAndQuestions(int projectId)
    {
        return _projectRepository.ReadProjectByIdWithWorkspaceTopicsYouthsAndQuestions(projectId) ?? throw new ProjectNotFoundException(projectId.ToString());
    }

    public Project GetProjectBySlug(Slug slug)
    {
        return _projectRepository.ReadProjectBySlug(slug) ?? throw new ProjectNotFoundException(slug.Text);
    }

    public Project GetProjectBySlugWithTopics(Slug slug)
    {
        return _projectRepository.ReadProjectBySlugWithTopics(slug) ?? throw new ProjectNotFoundException(slug.Text);
    }

    public Project GetProjectBySlugWithQuestions(Slug slug)
    {
        return _projectRepository.ReadProjectBySlugWithQuestions(slug) ?? throw new ProjectNotFoundException(slug.Text);
    }

    public Project GetProjectBySlugWithTopicsAndQuestions(Slug slug)
    {
        return _projectRepository.ReadProjectBySlugWithTopicsAndQuestions(slug) ?? throw new ProjectNotFoundException(slug.Text);
    }

    public Project GetProjectBySlugWithWorkspaceAndQuestions(Slug slug)
    {
        return _projectRepository.ReadProjectBySlugWithWorkspaceAndQuestions(slug) ?? throw new ProjectNotFoundException(slug.Text);
    }

    public Project GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug)
    {
        return _projectRepository.ReadProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(slug) ?? throw new ProjectNotFoundException(slug.Text);
    }

    public IReadOnlyCollection<Project> GetAllProjects()
    {
        return _projectRepository.ReadAllProjects();
    }

    public IReadOnlyCollection<Project> GetAllProjectsWithTopics()
    {
        return _projectRepository.ReadAllProjectsWithTopics();
    }

    public IReadOnlyCollection<Project> GetAllProjectsWithQuestions()
    {
        return _projectRepository.ReadAllProjectsWithQuestions();
    }

    public IReadOnlyCollection<Project> GetAllProjectsWithTopicsAndQuestions()
    {
        return _projectRepository.ReadAllProjectsWithTopicsAndQuestions();
    }

    public IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceId(int workspaceId)
    {
        return _projectRepository.ReadProjectsFromWorkspaceByWorkspaceId(workspaceId);
    }

    public IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithTopics(int workspaceId)
    {
        return _projectRepository.ReadProjectsFromWorkspaceByWorkspaceIdWithTopics(workspaceId);
    }

    public IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithQuestions(int workspaceId)
    {
        return _projectRepository.ReadProjectsFromWorkspaceByWorkspaceIdWithQuestions(workspaceId);
    }

    public IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(int workspaceId)
    {
        return _projectRepository.ReadProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(workspaceId);
    }

    public Project AddProject(string title, string slug, string description, Status status, DateTime startDate, DateTime endDate, InteractionType interactionForm, int workspaceId)
    {
        var workspace = _workspaceManager.GetWorkspaceById(workspaceId);
        var project = new Project
        {
            Title = title,
            Slug = Slug.FromName(title),
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
        if (SlugExists(project.Slug)) throw new ValidationException($"Project Slug '{project.Slug.Text}' already exists.");

        Validate(project);
        _projectRepository.CreateProject(project);
        return project;
    }

    public Project ChangeProject(Project project)
    {
        Validate(project);
        _projectRepository.UpdateProject(project);
        return project;
    }

    public void RemoveProject(int projectId)
    {
        if (!_projectRepository.DeleteProject(projectId))
        {
            throw new ProjectNotFoundException(projectId.ToString());
        }
    }

    public Topic GetTopicById(int topicId)
    {
        return _projectRepository.ReadTopicById(topicId) ?? throw new TopicNotFoundException(topicId.ToString());
    }

    public IReadOnlyCollection<Topic> GetTopicsFromProjectByProjectId(int projectId)
    {
        return _projectRepository.ReadTopicsFromProjectByProjectId(projectId);
    }

    public Topic AddTopic(string name, string context, int projectId)
    {
        var project = _projectRepository.ReadProjectById(projectId);
        if (project == null) throw new ProjectNotFoundException(projectId.ToString());

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

    public Topic ChangeTopic(Topic topic)
    {
        Validate(topic);
        _projectRepository.UpdateTopic(topic);
        return topic;
    }

    public void RemoveTopic(int topicId)
    {
        if (!_projectRepository.DeleteTopic(topicId))
        {
            throw new TopicNotFoundException(topicId.ToString());
        }
    }

    public Youth GetYouthByToken(string token)
    {
        return _projectRepository.ReadYouthByToken(token) ?? throw new YouthNotFoundException(token);
    }

    public IReadOnlyCollection<Youth> GetYouthsFromProjectByProjectId(int projectId)
    {
        return _projectRepository.ReadYouthsFromProjectByProjectId(projectId);
    }

    public Youth AddYouth(string token, string email, int projectId)
    {
        var project = _projectRepository.ReadProjectById(projectId);
        if (project == null) throw new ProjectNotFoundException(projectId.ToString());

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

    public Youth ChangeYouth(Youth youth)
    {
        Validate(youth);
        _projectRepository.UpdateYouth(youth);
        return youth;
    }

    public void RemoveYouth(string token)
    {
        if (!_projectRepository.DeleteYouth(token))
        {
            throw new YouthNotFoundException(token);
        }
    }
    
    private bool SlugExists(Slug slug)
    {
        return _projectRepository.ReadProjectBySlug(slug) != null;
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
