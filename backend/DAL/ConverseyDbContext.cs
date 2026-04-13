using Conversey.BL.Domain;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
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
    public DbSet<IdeaReaction> IdeaReactions { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<ResponseReaction> ResponseReactions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<AiAuditLog>  AiAuditLogs { get; set; }

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
            .Property(w => w.Id)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                slug => slug.Text,
                str => new Slug { Text = str });

        // Workspace 1-* Project
        modelBuilder.Entity<Workspace>()
            .HasMany(w => w.Projects)
            .WithOne(p => p.Workspace)
            .IsRequired();

        // Project
        modelBuilder.Entity<Project>()
            .HasKey(p => p.Slug);

        modelBuilder.Entity<Project>()
            .Property(p => p.Name)
            .HasMaxLength(100);
        
        modelBuilder.Entity<Project>()
            .Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                slug => slug.Text,
                str => new Slug { Text = str });

        modelBuilder.Entity<Project>()
            .Property(p => p.Description)
            .HasMaxLength(4000);

        modelBuilder.Entity<Project>()
            .Property(p => p.ImageUrl)
            .HasMaxLength(2048);

        // Project 1-* Topic
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Topic)
            .WithOne(t => t.Project);

        // Project 1-* Youth
        modelBuilder.Entity<Project>()
            .HasMany(p => p.Youth)
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

        // Question inheritance
        modelBuilder.Entity<OpenQuestion>();
        modelBuilder.Entity<ChoiceQuestion<SingleChoice>>();
        modelBuilder.Entity<ChoiceQuestion<MultipleChoice>>();
        modelBuilder.Entity<ScaleQuestion>();
        
        // Choice
        modelBuilder.Entity<SingleChoice>()
            .HasKey(c => c.Id);
        
        modelBuilder.Entity<MultipleChoice>()
            .HasKey(c => c.Id);
        
        modelBuilder.Entity<SingleChoice>()
            .Property(c => c.Text)
            .HasMaxLength(500);
        
        modelBuilder.Entity<MultipleChoice>()
            .Property(c => c.Text)
            .HasMaxLength(500);

        // Idea
        modelBuilder.Entity<Idea>()
            .HasKey(i => i.Id);

        modelBuilder.Entity<Idea>()
            .Property(i => i.Content)
            .IsRequired()
            .HasMaxLength(4000);

        modelBuilder.Entity<Idea>()
            .Property(i => i.Summary)
            .HasMaxLength(1000);

        modelBuilder.Entity<Idea>()
            .HasOne(i => i.Project)
            .WithMany()
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasOne(i => i.Topic)
            .WithMany(t => t.Ideas)
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasOne(i => i.Youth)
            .WithMany(y => y.Ideas)
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasMany(i => i.Responses)
            .WithOne(r => r.Idea)
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasMany(i => i.Reactions)
            .WithOne(ir => ir.Idea)
            .IsRequired();
        
        modelBuilder.Entity<Idea>()
            .Property(i => i.ModerationInfo)
            .HasConversion(m => m.Serialize(), b => ModerationInfo.Deserialize(b));

        // Reaction inheritance (base key must be defined on root type)
        modelBuilder.Entity<Reaction>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<Reaction>()
            .Property(r => r.Emoji)
            .IsRequired()
            .HasMaxLength(32);

        modelBuilder.Entity<Reaction>()
            .HasOne(r => r.Youth)
            .WithMany(y => y.Reactions)
            .IsRequired();

        // IdeaReaction
        modelBuilder.Entity<IdeaReaction>();

        // Response
        modelBuilder.Entity<Response>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<Response>()
            .Property(r => r.Text)
            .IsRequired()
            .HasMaxLength(4000);

        modelBuilder.Entity<Response>()
            .HasOne(r => r.Youth)
            .WithMany(y => y.Responses)
            .IsRequired();

        modelBuilder.Entity<Response>()
            .HasMany(r => r.Reactions)
            .WithOne(rr => rr.Response)
            .IsRequired();
        
        modelBuilder.Entity<Response>()
            .Property(r => r.ModerationInfo)
            .HasConversion(m => m.Serialize(), b => ModerationInfo.Deserialize(b));

        // ResponseReaction
        modelBuilder.Entity<ResponseReaction>();

        // Answer
        modelBuilder.Entity<Answer>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Youth)
            .WithMany(y => y.Answers)
            .IsRequired();


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
