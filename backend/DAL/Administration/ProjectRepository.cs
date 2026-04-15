using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conversey.DAL.Administration;

public class ProjectRepository : IProjectRepository
{

    private readonly ConverseyDbContext _dbContext;

    public ProjectRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Project ReadProjectById(Slug projectSlug)
    {
        return _dbContext.Projects
            .SingleOrDefault(p => p.Id == projectSlug);
    }

    public Project ReadProjectByIdWithTopics(Slug projectSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .SingleOrDefault(p => p.Id == projectSlug);
    }

    public Project ReadProjectByIdWithQuestions(Slug projectSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == projectSlug);
    }

    public Project ReadProjectByIdWithTopicsAndQuestions(Slug projectSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == projectSlug);
    }

    public Project ReadProjectByIdWithWorkspaceAndQuestions(Slug projectSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == projectSlug);
    }

    public Project ReadProjectByIdWithWorkspaceTopicsYouthsAndQuestions(Slug projectSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youth)
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == projectSlug);
    }

    public Project ReadProjectBySlug(Slug slug)
    {
        return _dbContext.Projects
            .SingleOrDefault(p => p.Id == slug);
    }

    public Project ReadProjectBySlugWithTopics(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .SingleOrDefault(p => p.Id == slug);
    }

    public Project ReadProjectBySlugWithQuestions(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == slug);
    }

    public Project ReadProjectBySlugWithTopicsAndQuestions(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == slug);
    }

    public Project ReadProjectBySlugWithWorkspaceAndQuestions(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == slug);
    }

    public Project ReadProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youth)
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == slug);
    }

    public IReadOnlyCollection<Project> ReadAllProjects()
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youth)
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
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadAllProjectsWithTopicsAndQuestions()
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Questions)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceId(Slug workspaceSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Youth)
            .Where(p => p.Workspace.Id == workspaceSlug)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithTopics(Slug workspaceSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Where(p => p.Workspace.Id == workspaceSlug)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithQuestions(Slug workspaceSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Questions)
            .Where(p => p.Workspace.Id == workspaceSlug)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(Slug workspaceSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .Include(p => p.Questions)
            .Where(p => p.Workspace.Id == workspaceSlug)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Topic> ReadTopicsFromProjectByProjectId(Slug projectSlug)
    {
        return _dbContext.Topics
            .Where(t => t.Project.Id == projectSlug)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Youth> ReadYouthsFromProjectByProjectId(Slug projectSlug)
    {
        return _dbContext.Youths
            .Where(y => y.Project.Id == projectSlug)
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

    public bool DeleteProject(Slug projectSlug)
    {
        var project = _dbContext.Projects
            .SingleOrDefault(p => p.Id == projectSlug);
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

    public Youth ReadYouthByToken(Guid token)
    {
        return _dbContext.Youths
            .SingleOrDefault(y => y.Id == token);
    }

    public Youth ReadYouthByTokenWithProject(Guid token)
    {
        return _dbContext.Youths
            .Include(y => y.Project)
            .SingleOrDefault(y => y.Id == token);
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

    public bool DeleteYouth(Guid token)
    {
        var youth = _dbContext.Youths
            .SingleOrDefault(y => y.Id == token);
        if (youth == null) return false;

        _dbContext.Youths.Remove(youth);
        _dbContext.SaveChanges();
        return true;
    }
}

#region ProjectConfig
public class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        #region Properties

        builder
            .HasKey(p => p.Id);

        builder
            .Property(p => p.Name)
            .HasMaxLength(100);
        
        builder
            .Property(p => p.Id)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                slug => slug.Text,
                str => new Slug { Text = str });

        builder
            .Property(p => p.Description)
            .HasMaxLength(4000);

        builder
            .Property(p => p.ImageUrl)
            .HasMaxLength(2048);

        #endregion


        #region Relations
        
        // Project 1-* Topic
        builder
            .HasMany(p => p.Topic)
            .WithOne(t => t.Project)
            .HasForeignKey("ProjectId");

        // Project 1-* Youth
        builder
            .HasMany(p => p.Youth)
            .WithOne(y => y.Project)
            .HasForeignKey("ProjectId");

        // Project 1-* Question
        builder
            .HasMany(p => p.Questions)
            .WithOne(q => q.Project)
            .HasForeignKey("ProjectId")
            .IsRequired();


        #endregion
    }
}
#endregion

#region TopicConfig
public class TopicConfig: IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.HasKey(t => t.Id);
    }
}
#endregion

#region YouthConfig
public class YouthConfig : IEntityTypeConfiguration<Youth>
{
    public void Configure(EntityTypeBuilder<Youth> builder)
    {
        builder.HasKey(y => y.Id);
    }
}
#endregion

