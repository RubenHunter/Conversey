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

    public Project ReadProjectByIdWithTopics(Slug projectSlug)
    {
        return _dbContext.Projects
            .Include(p => p.Topic)
            .SingleOrDefault(p => p.Id == projectSlug);
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

