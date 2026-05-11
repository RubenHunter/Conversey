using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Conversey.DAL.Survey;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL;

public class ConverseyDbContext : IdentityDbContext
{
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<ConverseyAdminUser> ConverseyAdmins { get; set; }
    public DbSet<WorkspaceAdminUser> WorkspaceAdmins { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Youth> Youths { get; set; }
    public DbSet<Idea> Ideas { get; set; }
    public DbSet<IdeaReaction> IdeaReactions { get; set; }
    public DbSet<IdeaResponse> Responses { get; set; }
    public DbSet<ResponseReaction> ResponseReactions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<AiAuditLog>  AiAuditLogs { get; set; }
    public DbSet<AiPrompt>  AiPrompts { get; set; }
    public DbSet<AiProviderConfig>  AiProviderConfigs { get; set; }
    public DbSet<RateLimitConfig>  RateLimitConfigs { get; set; }

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


        // Idea

        modelBuilder.ApplyConfiguration(new IdeaConfig());
        modelBuilder.ApplyConfiguration(new ReactionConfig());
        modelBuilder.ApplyConfiguration(new ResponseConfig());
        modelBuilder.ApplyConfiguration(new IdeaReactionConfig());
        modelBuilder.ApplyConfiguration(new ResponseReactionConfig());
        

        // WorkspaceAdmin
        // modelBuilder.Entity<WorkspaceAdmin>()
        //     .HasKey(wa => wa.Id);
        //
        // modelBuilder.Entity<WorkspaceAdmin>()
        //     .HasOne(wa => wa.Workspace);

        modelBuilder.Entity<WorkspaceAdminUser>()
            .HasOne(wa => wa.Workspace)
            .WithMany()
            .HasForeignKey("WorkspaceId")
            .IsRequired();

        
   

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
