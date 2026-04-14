using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Conversey.DAL.Survey;
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

        modelBuilder.ApplyConfiguration(new WorkspaceConfig());

        modelBuilder.ApplyConfiguration(new ProjectConfig());

        modelBuilder.ApplyConfiguration(new TopicConfig());

        modelBuilder.ApplyConfiguration(new YouthConfig());

        // Question 
        modelBuilder.ApplyConfiguration(new QuestionConfig());
        modelBuilder.ApplyConfiguration(new OpenQuestionConfig());
        modelBuilder.ApplyConfiguration(new ScaleQuestionConfig());
        modelBuilder.ApplyConfiguration(new SingleChoiceQuestionConfig());
        modelBuilder.ApplyConfiguration(new MultipleChoiceQuestionConfig());
        modelBuilder.ApplyConfiguration(new SingleChoiceConfig());
        modelBuilder.ApplyConfiguration(new MultipleChoiceConfig());
        modelBuilder.ApplyConfiguration(new AnswerStringConfig());
        modelBuilder.ApplyConfiguration(new AnswerIntConfig());
        modelBuilder.ApplyConfiguration(new AnswerSingleChoiceConfig());
        modelBuilder.ApplyConfiguration(new AnswerMultipleChoiceConfig());
        
        
        // modelBuilder.Entity<Question>()
        //     .HasKey(q => q.Id);
        //
        // modelBuilder.Entity<Question>()
        //     .Property(q => q.Text)
        //     .HasMaxLength(500);
        //
        // modelBuilder.Entity<OpenQuestion>();
        // modelBuilder.Entity<SingleChoiceQuestion>();
        // modelBuilder.Entity<MultipleChoiceQuestion>();
        // modelBuilder.Entity<ScaleQuestion>();
        //
        // modelBuilder.Entity<Question>()
        //     .HasMany(q => q.Options)
        //     .WithOne(o => o.Question)
        //     .IsRequired();
        //
        // modelBuilder.Entity<QuestionOption>()
        //     .HasKey(o => o.Id);
        //
        // modelBuilder.Entity<QuestionOption>()
        //     .Property(o => o.Text)
        //     .HasMaxLength(250);

        // Idea

        modelBuilder.ApplyConfiguration(new IdeaConfig());
        modelBuilder.ApplyConfiguration(new ReactionConfig());
        modelBuilder.ApplyConfiguration(new ResponseConfig());
        modelBuilder.ApplyConfiguration(new IdeaReactionConfig());
        modelBuilder.ApplyConfiguration(new ResponseReactionConfig());

        
        // TextAnswer

        // modelBuilder.Entity<Idea>()
        //     .HasKey(i => i.Id);
        //
        // modelBuilder.Entity<Idea>()
        //     .Property(i => i.Content)
        //     .IsRequired()
        //     .HasMaxLength(4000);
        //
        // modelBuilder.Entity<Idea>()
        //     .Property(i => i.Summary)
        //     .HasMaxLength(1000);
        //
        //
        //
        // modelBuilder.Entity<Idea>()
        //     .HasOne(i => i.Youth)
        //     .WithMany(y => y.Ideas)
        //     .HasForeignKey("YouthToken")
        //     .IsRequired();
        //
        //     modelBuilder.Entity<Idea>()
        //         .HasMany(i => i.Responses)
        //         .WithOne(r => r.Idea)
        //         .HasForeignKey("IdeaId")
        //         .IsRequired();
        //
        // modelBuilder.Entity<Idea>()
        //     .HasMany(i => i.Reactions)
        //     .WithOne(ir => ir.Idea)
        //     .HasForeignKey("IdeaId")
        //     .IsRequired();
        //
        // modelBuilder.Entity<Idea>()
        //     .Property(i => i.ModerationInfo)
        //     .HasConversion(m => m.Serialize(), b => ModerationInfo.Deserialize(b));
        //
        // // IdeaReaction
        // modelBuilder.Entity<IdeaReaction>()
        //     .HasKey(ir => ir.Id);
        //
        // modelBuilder.Entity<IdeaReaction>()
        //     .Property(ir => ir.Emoji)
        //     .IsRequired()
        //     .HasMaxLength(32);
        //
        // modelBuilder.Entity<IdeaReaction>()
        //     .HasOne(ir => ir.Youth)
        //     .WithMany(y => y.IdeaReactions)
        //     .HasForeignKey("YouthToken")
        //     .IsRequired();
        //
        // modelBuilder.Entity<IdeaReaction>()
        //     .HasIndex(ir => new { ir.IdeaId, ir.YouthToken, ir.Emoji })
        //     .IsUnique();
        //
        // // Response
        // modelBuilder.Entity<Response>()
        //     .HasKey(r => r.Id);
        //
        // modelBuilder.Entity<Response>()
        //     .Property(r => r.Text)
        //     .IsRequired()
        //     .HasMaxLength(4000);
        //
        // modelBuilder.Entity<Response>()
        //     .HasOne(r => r.Youth)
        //     .WithMany(y => y.Responses)
        //     .HasForeignKey("YouthToken")
        //     .IsRequired();
        //
        // modelBuilder.Entity<Response>()
        //     .HasMany(r => r.Reactions)
        //     .WithOne(rr => rr.Response)
        //     .HasForeignKey("ResponseId")
        //     .IsRequired();
        //
        // modelBuilder.Entity<Response>()
        //     .Property(r => r.ModerationInfo)
        //     .HasConversion(m => m.Serialize(), b => ModerationInfo.Deserialize(b));
        //
        // // ResponseReaction
        // modelBuilder.Entity<ResponseReaction>()
        //     .HasKey(rr => rr.Id);
        //
        // modelBuilder.Entity<ResponseReaction>()
        //     .Property(rr => rr.Emoji)
        //     .IsRequired()
        //     .HasMaxLength(32);
        //
        // modelBuilder.Entity<ResponseReaction>()
        //     .HasOne(rr => rr.Youth)
        //     .WithMany(y => y.ResponseReactions)
        //     .HasForeignKey("YouthToken")
        //     .IsRequired();
        //
        // modelBuilder.Entity<ResponseReaction>()
        //     .HasIndex(rr => new { rr.ResponseId, rr.YouthToken, rr.Emoji })
        //     .IsUnique();


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
