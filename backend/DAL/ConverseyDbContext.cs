using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL;

public class ConverseyDbContext : DbContext
{
    
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<WorkspaceAdmin> WorkspaceAdmins { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Youth> Youths { get; set; }
    public DbSet<Idea> Ideas { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<TextAnswer> TextAnswers { get; set; }
    public DbSet<IntegerAnswer> IntegerAnswers { get; set; }
    
    public ConverseyDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Workspace
        modelBuilder.Entity<Workspace>()
            .HasKey(w => w.Id);

        modelBuilder.Entity<Workspace>()
            .Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Workspace>()
            .Property(w => w.Slug)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                slug => slug.ToString(),
                str => Slug.FromName(str));


        // Workspace 1-* Project
        modelBuilder.Entity<Workspace>()
            .HasMany(w => w.Projects)
            .WithOne(p => p.Workspace)
            .IsRequired();

        // Project
        modelBuilder.Entity<Project>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Project>()
            .Property(p => p.Title)
            .HasMaxLength(100);
        
        modelBuilder.Entity<Project>()
            .Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                slug => slug.ToString(),
                str => Slug.FromName(str));

        modelBuilder.Entity<Project>()
            .Property(p => p.Description)
            .HasMaxLength(4000);

        // Project 1-* Topic
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Topic)
            .WithOne(t => t.Project);

        // Project 1-* Youth
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Youths)
            .WithOne(y => y.Project);

        // Project 1-* Question
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Questions)
            .WithOne(q => q.Project);

        // Topic
        modelBuilder.Entity<Topic>()
            .HasKey(t => t.Id);

        // Youth
        modelBuilder.Entity<Youth>()
            .HasKey(y => y.Token);

        // Question 
        modelBuilder.Entity<Question>()
            .HasKey(q => q.Id);

        modelBuilder.Entity<Question>()
            .Property(q => q.Text)
            .HasMaxLength(500);

        // Idea
        modelBuilder.Entity<Idea>()
            .HasKey(i => i.Id);

        // Project 1-* Idea
        modelBuilder.Entity<Idea>()
            .HasOne(i => i.Project)
            .WithMany()
            .IsRequired();

        // Idea 1-* Response
        modelBuilder.Entity<Idea>()
            .HasMany(i => i.Responses)
            .WithOne(r => r.Idea);

        // Response
        modelBuilder.Entity<Response>()
            .HasKey(r => r.Id);

        // TextAnswer
        modelBuilder.Entity<TextAnswer>()
            .HasKey(a => a.Id);

        // IntegerAnswer
        modelBuilder.Entity<IntegerAnswer>()
            .HasKey(a => a.Id);

        // WorkspaceAdmin
        modelBuilder.Entity<WorkspaceAdmin>()
            .HasKey(wa => wa.Id);

        modelBuilder.Entity<WorkspaceAdmin>()
            .HasOne(wa => wa.Workspace);
    }

    public bool CreateDatabase(bool resetDatabase)
    {
        if (resetDatabase)
        {
            Database.EnsureDeleted();
        }

        return Database.EnsureCreated();
    }
}