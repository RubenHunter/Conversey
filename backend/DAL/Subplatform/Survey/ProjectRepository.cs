using Conversey.BL.Domain.Subplatform.Survey;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Survey;

public class ProjectRepository : IProjectRepository
{

    private readonly ConverseyDbContext _dbContext;

    public ProjectRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Project ReadProjectById(int projectId)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youths)
            .Include(p => p.Questions)
            .Single(p => p.Id == projectId) ?? throw new KeyNotFoundException($"Project with id {projectId} not found.");
    }

    public IReadOnlyCollection<Project> ReadAllProjects()
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youths)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsByWorkspaceId(int workspaceId)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Youths)
            .Where(p => p.Workspace.Id == workspaceId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Topic> ReadTopicsByProjectId(int projectId)
    {
        return _dbContext.Topics
            .Where(t => t.Project.Id == projectId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Youth> ReadYouthsByProjectId(int projectId)
    {
        return _dbContext.Youths
            .Where(y => y.Project.Id == projectId)
            .ToList().AsReadOnly();
    }

    public void CreateProject(Project project)
    {
        _dbContext.Projects.Add(project);
        _dbContext.SaveChanges();
    }

    public void UpdateProject(Project project)
    {
        _dbContext.Projects.Update(project);
        _dbContext.SaveChanges();
    }

    public void DeleteProject(int projectId)
    {
        var project = _dbContext.Projects.Find(projectId)
            ?? throw new KeyNotFoundException($"Project with id {projectId} not found.");
        _dbContext.Projects.Remove(project);
        _dbContext.SaveChanges();
    }

    public Topic ReadTopicById(int topicId)
    {
        return _dbContext.Topics
            .Include(t => t.Project)
            .FirstOrDefault(t => t.Id == topicId)
            ?? throw new KeyNotFoundException($"Topic with id {topicId} not found.");
    }

    public void CreateTopic(Topic topic)
    {
        _dbContext.Topics.Add(topic);
        _dbContext.SaveChanges();
    }

    public void UpdateTopic(Topic topic)
    {
        _dbContext.Topics.Update(topic);
        _dbContext.SaveChanges();
    }

    public void DeleteTopic(int topicId)
    {
        var topic = _dbContext.Topics.Find(topicId)
            ?? throw new KeyNotFoundException($"Topic with id {topicId} not found.");
        _dbContext.Topics.Remove(topic);
        _dbContext.SaveChanges();
    }

    public Youth ReadYouthByToken(string token)
    {
        return _dbContext.Youths
            .Include(y => y.Project)
            .FirstOrDefault(y => y.Token == token)
            ?? throw new KeyNotFoundException($"Youth with token {token} not found.");
    }

    public void CreateYouth(Youth youth)
    {
        _dbContext.Youths.Add(youth);
        _dbContext.SaveChanges();
    }

    public void UpdateYouth(Youth youth)
    {
        _dbContext.Youths.Update(youth);
        _dbContext.SaveChanges();
    }

    public void DeleteYouth(string token)
    {
        var youth = _dbContext.Youths.Find(token)
            ?? throw new KeyNotFoundException($"Youth with token {token} not found.");
        _dbContext.Youths.Remove(youth);
        _dbContext.SaveChanges();
    }
}
