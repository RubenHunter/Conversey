using Conversey.BL.Domain;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

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
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<TextAnswer> TextAnswers { get; set; }
    public DbSet<OpenTextAnswer> OpenTextAnswers { get; set; }
    public DbSet<ClosedTextAnswer> ClosedTextAnswers { get; set; }
    public DbSet<IntegerAnswer> IntegerAnswers { get; set; }
    public DbSet<AiAuditLog>  AiAuditLogs { get; set; }
    public DbSet<AiPrompt> AiPrompts { get; set; }

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
            .HasForeignKey("ProjectId")   
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasOne(i => i.Topic)
            .WithMany(t => t.Ideas)
            .HasForeignKey("TopicId")
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasOne(i => i.Youth)
            .WithMany(y => y.Ideas)
            .HasForeignKey("YouthToken")
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasMany(i => i.Responses)
            .WithOne(r => r.Idea)
            .HasForeignKey("IdeaId")
            .IsRequired();

        modelBuilder.Entity<Idea>()
            .HasMany(i => i.Reactions)
            .WithOne(ir => ir.Idea)
            .HasForeignKey("IdeaId")
            .IsRequired();
        
        modelBuilder.Entity<Idea>()
            .Property(i => i.ModerationInfo)
            .HasConversion(m => m.Serialize(), b => ModerationInfo.Deserialize(b));

        // IdeaReaction
        modelBuilder.Entity<IdeaReaction>()
            .HasKey(ir => ir.Id);

        modelBuilder.Entity<IdeaReaction>()
            .Property(ir => ir.Emoji)
            .IsRequired()
            .HasMaxLength(32);

        modelBuilder.Entity<IdeaReaction>()
            .HasOne(ir => ir.Youth)
            .WithMany(y => y.IdeaReactions)
            .HasForeignKey("YouthToken")
            .IsRequired();

        modelBuilder.Entity<IdeaReaction>()
            .HasIndex(ir => new { ir.IdeaId, ir.YouthToken, ir.Emoji })
            .IsUnique();

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
            .HasForeignKey("YouthToken")
            .IsRequired();

        modelBuilder.Entity<Response>()
            .HasMany(r => r.Reactions)
            .WithOne(rr => rr.Response)
            .HasForeignKey("ResponseId")
            .IsRequired();
        
        modelBuilder.Entity<Response>()
            .Property(r => r.ModerationInfo)
            .HasConversion(m => m.Serialize(), b => ModerationInfo.Deserialize(b));

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
            .HasForeignKey("YouthToken")
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
            .HasForeignKey("YouthToken")
            .IsRequired();

        modelBuilder.Entity<TextAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey("QuestionId")
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
            .HasForeignKey("YouthToken")
            .IsRequired();

        modelBuilder.Entity<IntegerAnswer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey("QuestionId")
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
