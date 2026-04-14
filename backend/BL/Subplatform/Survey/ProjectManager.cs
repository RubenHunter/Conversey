using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Subplatform;
using Conversey.DAL.Administration;

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

    public Project GetProjectById(Slug projectSlug)
    {
        return _projectRepository.ReadProjectById(projectSlug) ?? throw new ProjectNotFoundException(projectSlug.Text);
    }

    public Project GetProjectByIdWithTopics(Slug projectSlug)
    {
        return _projectRepository.ReadProjectByIdWithTopics(projectSlug) ?? throw new ProjectNotFoundException(projectSlug.Text);
    }

    public Project GetProjectByIdWithQuestions(Slug projectSlug)
    {
        return _projectRepository.ReadProjectByIdWithQuestions(projectSlug) ?? throw new ProjectNotFoundException(projectSlug.Text);
    }

    public Project GetProjectByIdWithTopicsAndQuestions(Slug projectSlug)
    {
        return _projectRepository.ReadProjectByIdWithTopicsAndQuestions(projectSlug) ?? throw new ProjectNotFoundException(projectSlug.Text);
    }

    public Project GetProjectByIdWithWorkspaceAndQuestions(Slug projectSlug)
    {
        return _projectRepository.ReadProjectByIdWithWorkspaceAndQuestions(projectSlug) ?? throw new ProjectNotFoundException(projectSlug.Text);
    }

    public Project GetProjectByIdWithWorkspaceTopicsYouthsAndQuestions(Slug projectSlug)
    {
        return _projectRepository.ReadProjectByIdWithWorkspaceTopicsYouthsAndQuestions(projectSlug) ?? throw new ProjectNotFoundException(projectSlug.Text);
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

    public IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceId(Slug workspaceSlug)
    {
        return _projectRepository.ReadProjectsFromWorkspaceByWorkspaceId(workspaceSlug);
    }

    public IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithTopics(Slug workspaceSlug)
    {
        return _projectRepository.ReadProjectsFromWorkspaceByWorkspaceIdWithTopics(workspaceSlug);
    }

    public IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithQuestions(Slug workspaceSlug)
    {
        return _projectRepository.ReadProjectsFromWorkspaceByWorkspaceIdWithQuestions(workspaceSlug);
    }

    public IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(Slug workspaceSlug)
    {
        return _projectRepository.ReadProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(workspaceSlug);
    }

    public Project AddProject(string title, string slug, string description, Status status, DateTime startDate, DateTime endDate, InteractionType interactionForm, Slug workspaceSlug)
    {
        var workspace = _workspaceManager.GetWorkspaceBySlug(workspaceSlug);
        var project = new Project
        {
            Name = title,
            Slug = Slug.FromName(title),
            Description = description,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            InteractionForm = interactionForm,
            Workspace = workspace,
            Topic = new List<Topic>(),
            Questions = new List<Question>(),
            Youth = new List<Youth>()
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

    public void RemoveProject(Slug projectSlug)
    {
        if (!_projectRepository.DeleteProject(projectSlug))
        {
            throw new ProjectNotFoundException(projectSlug.Text);
        }
    }

    public Topic GetTopicById(int topicId)
    {
        return _projectRepository.ReadTopicById(topicId) ?? throw new TopicNotFoundException(topicId.ToString());
    }

    public IReadOnlyCollection<Topic> GetTopicsFromProjectByProjectId(Slug projectSlug)
    {
        return _projectRepository.ReadTopicsFromProjectByProjectId(projectSlug);
    }


    public Topic AddTopic(string name, string context, Slug projectSlug)
    {
        var project = _projectRepository.ReadProjectById(projectSlug);
        if (project == null) throw new ProjectNotFoundException(projectSlug.Text);

        var topic = new Topic
        {
            Name = name,
            Context = context,
            Project = project,
            Ideas = new List<Idea>()
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

    public Youth GetYouthByToken(Guid token)
    {
        return _projectRepository.ReadYouthByToken(token) ?? throw new YouthNotFoundException(token.ToString());
    }

    public IReadOnlyCollection<Youth> GetYouthsFromProjectByProjectId(Slug projectSlug)
    {
        return _projectRepository.ReadYouthsFromProjectByProjectId(projectSlug);
    }

    public Youth AddYouth(Guid token, string email, Slug projectSlug)
    {
        var project = _projectRepository.ReadProjectById(projectSlug);
        if (project == null) throw new ProjectNotFoundException(projectSlug.Text);

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

    public void RemoveYouth(Guid token)
    {
        if (!_projectRepository.DeleteYouth(token))
        {
            throw new YouthNotFoundException(token.ToString());
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
