using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
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

    public Project ReadProjectByIdAndWorkspaceId(Slug projectId, Slug workspaceId)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .SingleOrDefault(p => p.Id == projectId && EF.Property<Slug>(p, "WorkspaceId") == workspaceId);
    }


    public Project ReadProjectByIdWithWorkspaceAndTopicsAndYouthAndQuestions(Slug projectId)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Topic)
            .Include(p => p.Youth)
            .Include(p => p.Questions)
            .SingleOrDefault(p => p.Id == projectId);
    }

    public Youth ReadYouthByIdAndProjectId(Guid youthId, Slug projectId)
    {
        return _dbContext.Youths
            .SingleOrDefault(y => y.Id == youthId && EF.Property<Slug>(y, "ProjectId") == projectId);
    }

    public Topic ReadTopicByIdAndProjectId(int topicId, Slug projectId)
    {
        return _dbContext.Topics
            .SingleOrDefault(y => y.Id == topicId && EF.Property<Slug>(y, "ProjectId") == projectId);
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

    public IReadOnlyCollection<Project> ReadAllProjectsFromWorkspaceId(Slug workspaceId)
    {
        return _dbContext.Projects
            .Include(p => p.Workspace)
            .Where(p => p.Workspace.Id == workspaceId)
            .ToList()
            .AsReadOnly();
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

    public void DeleteProject(Slug projectId, Slug workspaceId)
    {
        var project = ReadProjectByIdAndWorkspaceId(projectId, workspaceId);
        _dbContext.Projects.Remove(project);
        _dbContext.SaveChanges();
    }

    public void DeleteAllProjectsFromWorkspaceId(Slug workspaceId)
    {
        var projects = ReadAllProjectsFromWorkspaceId(workspaceId);
        _dbContext.Projects.RemoveRange(projects);
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

        builder
            .Property(p => p.NudgingStrength)
            .HasDefaultValue(3);

        builder
            .Property(p => p.Language)
            .HasMaxLength(5)
            .HasDefaultValue("nl");

        #endregion


        #region Relations
        
        // Project 1-* Topic
        builder
            .HasMany(p => p.Topic)
            .WithOne(t => t.Project)
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);

        // Project 1-* Youth
        builder
            .HasMany(p => p.Youth)
            .WithOne(y => y.Project)
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);

        // Project 1-* Question
        builder
            .HasMany(p => p.Questions)
            .WithOne(q => q.Project)
            .HasForeignKey("ProjectId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);


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

