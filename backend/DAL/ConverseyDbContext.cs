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
    public DbSet<ResponseReaction> ResponseReactions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<TextAnswer> TextAnswers { get; set; }
    public DbSet<OpenTextAnswer> OpenTextAnswers { get; set; }
    public DbSet<ClosedTextAnswer> ClosedTextAnswers { get; set; }
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
                slug => slug.Text,
                str => new Slug { Text = str });


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

        modelBuilder.Entity<OpenQuestion>();
        modelBuilder.Entity<SingleChoiceQuestion>();
        modelBuilder.Entity<MultipleChoiceQuestion>();
        modelBuilder.Entity<ScaleQuestion>();

        modelBuilder.Entity<Question>()
            .HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .IsRequired();

        modelBuilder.Entity<QuestionOption>()
            .HasKey(o => o.Id);

        modelBuilder.Entity<QuestionOption>()
            .Property(o => o.Text)
            .HasMaxLength(250);

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
            .HasForeignKey(i => i.ProjectId)
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasOne(i => i.Topic)
            .WithMany(t => t.Ideas)
            .HasForeignKey(i => i.TopicId)
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasOne(i => i.Youth)
            .WithMany(y => y.Ideas)
            .HasForeignKey(i => i.YouthToken)
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasMany(i => i.Responses)
            .WithOne(r => r.Idea)
            .HasForeignKey(r => r.IdeaId)
            .IsRequired();

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
            .HasForeignKey(r => r.YouthToken)
            .IsRequired();

        modelBuilder.Entity<Response>()
            .HasMany(r => r.Reactions)
            .WithOne(rr => rr.Response)
            .HasForeignKey(rr => rr.ResponseId)
            .IsRequired();

        // ResponseReaction
        modelBuilder.Entity<ResponseReaction>()
            .HasKey(rr => rr.Id);

        modelBuilder.Entity<ResponseReaction>()
            .Property(rr => rr.Emoji)
            .IsRequired()
            .HasMaxLength(32);

        modelBuilder.Entity<ResponseReaction>()
            .HasOne(rr => rr.Youth)
            .WithMany(y => y.ResponseReactions)
            .HasForeignKey(rr => rr.YouthToken)
            .IsRequired();

        modelBuilder.Entity<ResponseReaction>()
            .HasIndex(rr => new { rr.ResponseId, rr.YouthToken, rr.Emoji })
            .IsUnique();

        // TextAnswer
        modelBuilder.Entity<TextAnswer>()
            .ToTable("TextAnswers")
            .HasKey(a => a.Id);

        modelBuilder.Entity<TextAnswer>()
            .HasOne(a => a.Youth)
            .WithMany()
            .HasForeignKey(a => a.YouthToken)
            .IsRequired();

        modelBuilder.Entity<TextAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .IsRequired();

        modelBuilder.Entity<OpenTextAnswer>()
            .ToTable("OpenTextAnswers");

        modelBuilder.Entity<ClosedTextAnswer>()
            .ToTable("ClosedTextAnswers");

        // IntegerAnswer
        modelBuilder.Entity<IntegerAnswer>()
            .HasKey(a => a.Id);

        modelBuilder.Entity<IntegerAnswer>()
            .HasOne(a => a.Youth)
            .WithMany()
            .HasForeignKey(a => a.YouthToken)
            .IsRequired();

        modelBuilder.Entity<IntegerAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
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