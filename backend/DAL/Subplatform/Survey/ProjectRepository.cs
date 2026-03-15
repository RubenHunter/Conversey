﻿using Conversey.BL.Domain.Common;
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
            .SingleOrDefault(p => p.Id == projectId);
    }

    public Project ReadProjectByIdWithTopics(int projectId)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .SingleOrDefault(p => p.Id == projectId);
    }

    public Project ReadProjectByIdWithQuestions(int projectId)
    {
        return _dbContext.Projects
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefault(p => p.Id == projectId);
    }

    public Project ReadProjectByIdWithTopicsAndQuestions(int projectId)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefault(p => p.Id == projectId);
    }

    public Project ReadProjectByIdWithWorkspaceAndQuestions(int projectId)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefault(p => p.Id == projectId);
    }

    public Project ReadProjectByIdWithWorkspaceTopicsYouthsAndQuestions(int projectId)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youths)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefault(p => p.Id == projectId);
    }

    public Project ReadProjectBySlug(Slug slug)
    {
        return _dbContext.Projects
            .SingleOrDefault(p => p.Slug == slug);
    }

    public Project ReadProjectBySlugWithTopics(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .SingleOrDefault(p => p.Slug == slug);
    }

    public Project ReadProjectBySlugWithQuestions(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefault(p => p.Slug == slug);
    }

    public Project ReadProjectBySlugWithTopicsAndQuestions(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefault(p => p.Slug == slug);
    }

    public Project ReadProjectBySlugWithWorkspaceAndQuestions(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefault(p => p.Slug == slug);
    }

    public Project ReadProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youths)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .SingleOrDefault(p => p.Slug == slug);
    }

    public IReadOnlyCollection<Project> ReadAllProjects()
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youths)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadAllProjectsWithTopics()
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadAllProjectsWithQuestions()
    {
        return _dbContext.Projects
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadAllProjectsWithTopicsAndQuestions()
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceId(int workspaceId)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Youths)
            .Where(p => p.Workspace.Id == workspaceId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithTopics(int workspaceId)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Where(p => p.Workspace.Id == workspaceId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithQuestions(int workspaceId)
    {
        return _dbContext.Projects
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .Where(p => p.Workspace.Id == workspaceId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(int workspaceId)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Questions)
            .ThenInclude(q => q.Options)
            .Where(p => p.Workspace.Id == workspaceId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Topic> ReadTopicsFromProjectByProjectId(int projectId)
    {
        return _dbContext.Topics
            .Where(t => t.Project.Id == projectId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Youth> ReadYouthsFromProjectByProjectId(int projectId)
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

    public bool DeleteProject(int projectId)
    {
        var project = _dbContext.Projects
            .SingleOrDefault(p => p.Id == projectId);
        if (project == null) return false;

        _dbContext.Projects.Remove(project);
        _dbContext.SaveChanges();
        return true;
    }

    public Topic ReadTopicById(int topicId)
    {
        return _dbContext.Topics
            .SingleOrDefault(t => t.Id == topicId);
    }

    public Topic ReadTopicByIdWithProject(int topicId)
    {
        return _dbContext.Topics
            .Include(t => t.Project)
            .SingleOrDefault(t => t.Id == topicId);
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

    public bool DeleteTopic(int topicId)
    {
        var topic = _dbContext.Topics
            .SingleOrDefault(t => t.Id == topicId);
        if (topic == null) return false;

        _dbContext.Topics.Remove(topic);
        _dbContext.SaveChanges();
        return true;
    }

    public Youth ReadYouthByToken(string token)
    {
        return _dbContext.Youths
            .SingleOrDefault(y => y.Token == token);
    }

    public Youth ReadYouthByTokenWithProject(string token)
    {
        return _dbContext.Youths
            .Include(y => y.Project)
            .SingleOrDefault(y => y.Token == token);
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

    public bool DeleteYouth(string token)
    {
        var youth = _dbContext.Youths
            .SingleOrDefault(y => y.Token == token);
        if (youth == null) return false;

        _dbContext.Youths.Remove(youth);
        _dbContext.SaveChanges();
        return true;
    }
}

