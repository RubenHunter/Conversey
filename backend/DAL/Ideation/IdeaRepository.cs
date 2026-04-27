using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Conversey.DAL.Ideation;

public class IdeaRepository : IIdeaRepository
{
    private readonly ConverseyDbContext _dbContext;

    public IdeaRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void CreateIdea(Idea idea)
    {
        _dbContext.Ideas.Add(idea);
        _dbContext.SaveChanges();
    }

    public Idea ReadIdeaById(int ideaId)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .ThenInclude(p => p.Workspace)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .SingleOrDefault(i => i.Id == ideaId);
    }

    public Idea ReadIdeaByIdWithProjectAndResponses(int ideaId)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Reactions)
            .SingleOrDefault(i => i.Id == ideaId);
    }

    public IReadOnlyCollection<Idea> ReadIdeasByYouthId(Guid youthId)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .Where(i => i.Youth.Id == youthId)
            .OrderByDescending(i => i.SubmissionDate)
            .ThenByDescending(i => i.Id)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasByTopicId(int topicId)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .Where(i => i.Topic.Id == topicId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasByTopicIdAndStatus(int topicId, ModerationStatus status)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .ThenInclude(p => p.Workspace)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .Where(i => i.Topic.Id == topicId && i.Status == status)
            .OrderByDescending(i => i.SubmissionDate)
            .ThenByDescending(i => i.Id)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasByTopicIdAndYouthId(int topicId, Guid youthId)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .ThenInclude(p => p.Workspace)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .Where(i => i.Topic.Id == topicId && i.Youth.Id == youthId)
            .OrderByDescending(i => i.SubmissionDate)
            .ThenByDescending(i => i.Id)
            .ToList()
            .AsReadOnly();
    }

    public void UpdateIdea(Idea idea)
    {
        _dbContext.Ideas.Update(idea);
        _dbContext.SaveChanges();
    }

    public void CreateResponse(IdeaResponse ideaResponse)
    {
        _dbContext.Responses.Add(ideaResponse);
        _dbContext.SaveChanges();
    }

    public IdeaResponse ReadResponseById(int responseId)
    {
        return _dbContext.Responses
            .Include(r => r.Youth)
            .Include(r => r.Reactions)
            .SingleOrDefault(r => r.Id == responseId);
    }

    public IdeaResponse ReadResponseByIdWithIdea(int responseId)
    {
        return _dbContext.Responses
            .Include(r => r.Idea)
            .ThenInclude(i => i.Project)
            .Include(r => r.Youth)
            .Include(r => r.Reactions)
            .SingleOrDefault(r => r.Id == responseId);
    }

    public IReadOnlyCollection<IdeaResponse> ReadResponsesByIdeaId(int ideaId)
    {
        return _dbContext.Responses
            .Include(r => r.Youth)
            .Include(r => r.Reactions)
            .Where(r => r.Idea.Id == ideaId)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .ToList().AsReadOnly();
    }

    public void UpdateResponse(IdeaResponse ideaResponse)
    {
        _dbContext.Responses.Update(ideaResponse);
        _dbContext.SaveChanges();
    }


    public void CreateIdeaReaction(IdeaReaction reaction)
    {
        _dbContext.IdeaReactions.Add(reaction);
        _dbContext.SaveChanges();
    }

    public IdeaReaction ReadIdeaReactionByIdeaIdAndYouthIdAndEmoji(int ideaId, Guid youthId, string emoji)
    {
        return _dbContext.IdeaReactions
            .SingleOrDefault(ir => ir.Idea.Id == ideaId && ir.Youth.Id == youthId && ir.Emoji == emoji);
    }

    public IReadOnlyCollection<IdeaReaction> ReadIdeaReactionsByIdeaId(int ideaId)
    {
        return _dbContext.IdeaReactions
            .Include(ir => ir.Youth)
            .Where(ir => ir.Idea.Id == ideaId)
            .OrderBy(ir => ir.Emoji)
            .ThenBy(ir => ir.Id)
            .ToList().AsReadOnly();
    }

    public bool DeleteIdeaReaction(int reactionId)
    {
        var reaction = _dbContext.IdeaReactions
            .SingleOrDefault(r => r.Id == reactionId);

        if (reaction == null) return false;

        _dbContext.IdeaReactions.Remove(reaction);
        _dbContext.SaveChanges();
        return true;
    }

    public void CreateResponseReaction(ResponseReaction reaction)
    {
        _dbContext.ResponseReactions.Add(reaction);
        _dbContext.SaveChanges();
    }
    
    public ResponseReaction ReadResponseReactionByReactionId(int reactionId)
    {
        return _dbContext.ResponseReactions
            .SingleOrDefault(rr =>
                rr.Id == reactionId);
    }

    public ResponseReaction ReadResponseReactionByResponseIdAndYouthIdAndEmoji(int responseId, Guid youthId, string emoji)
    {
        return _dbContext.ResponseReactions
            .SingleOrDefault(rr =>
                EF.Property<int>(rr, "ResponseId") == responseId &&
                rr.Youth.Id == youthId &&
                rr.Emoji == emoji);
    }

    public IReadOnlyCollection<ResponseReaction> ReadResponseReactionsByResponseId(int responseId)
    {
        return _dbContext.ResponseReactions
            .Where(rr => EF.Property<int>(rr, "ResponseId") == responseId)
            .OrderBy(rr => rr.Emoji)
            .ThenBy(rr => rr.Id)
            .ToList().AsReadOnly();
    }

    public IEnumerable<IdeaResponse> ReadApprovedResponsesByYouthIdAndIdeaId(int ideaId, Guid youthId)
    {
        return _dbContext.Responses
            .Include(r => r.Idea)
            .Include(r => r.Youth)
            .Include(r => r.Reactions)
            .Where(r => r.Idea.Id == ideaId && r.Youth.Id == youthId && r.Status == ModerationStatus.Approved);
    }

    public bool DeleteResponseReaction(int reactionId)
    {
        var reaction = _dbContext.ResponseReactions
            .SingleOrDefault(rr =>
                rr.Id == reactionId);
        if (reaction == null) return false;

        _dbContext.ResponseReactions.Remove(reaction);
        _dbContext.SaveChanges();
        return true;
    }
}

public class IdeaConfig : IEntityTypeConfiguration<Idea> 
{
    public void Configure(EntityTypeBuilder<Idea> builder)
    {
        #region Properies
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(i => i.Summary)
            .HasMaxLength(1000);

        builder.Property(i => i.SemanticCategories)
            .HasConversion(
                categories => JsonSerializer.Serialize(categories ?? Array.Empty<string>(), (JsonSerializerOptions)null),
                serialized => string.IsNullOrWhiteSpace(serialized)
                    ? Array.Empty<string>()
                    : JsonSerializer.Deserialize<string[]>(serialized) ?? Array.Empty<string>())
            .HasColumnType("text");

        builder.Property(i => i.SubmissionDate);

        builder.Property(i => i.Status);

        builder.Property(i => i.ModerationInfo)
            .HasConversion(
                m => ModerationInfoSerializer.Serialize(m),
                b => ModerationInfoSerializer.Deserialize(b)
            );
        #endregion

        #region Relations

        builder.HasOne(i => i.Project)
            .WithMany(p => p.ProjectIdeas)
            .HasForeignKey("ProjectId")
            .IsRequired();

        builder.HasOne(i => i.Topic)
            .WithMany(t => t.Ideas)
            .HasForeignKey("TopicId")
            .IsRequired();

        builder.HasOne(i => i.Youth)
            .WithMany(y => y.Ideas)
            .HasForeignKey("YouthToken")
            .IsRequired();

        builder.HasMany(i => i.Responses)
            .WithOne(r => r.Idea)
            .HasForeignKey("IdeaId");

        builder.HasMany(i => i.Reactions)
            .WithOne(ir => ir.Idea)
            .HasForeignKey("IdeaId");

        #endregion
    }
}

public class ReactionConfig : IEntityTypeConfiguration<Reaction>
 {
     public void Configure(EntityTypeBuilder<Reaction> builder)
     {
         #region Properties

         builder.HasKey(r => r.Id);
         builder.Property(r => r.Emoji)
             .IsRequired()
             .HasMaxLength(32);
         builder.Property(r => r.CreatedAt);

         #endregion

         #region Relations

         builder.HasOne(r => r.Youth)
             .WithMany(y => y.Reactions)
             .HasForeignKey("YouthToken")
             .IsRequired();
        
         #endregion
     }
}

public class ResponseConfig : IEntityTypeConfiguration<IdeaResponse>
{
    public void Configure(EntityTypeBuilder<IdeaResponse> builder)
    {
        builder.Property(r => r.ModerationInfo)
            .HasConversion(
                m => ModerationInfoSerializer.Serialize(m),
                b => ModerationInfoSerializer.Deserialize(b)
            );
    }
}

public class IdeaReactionConfig : IEntityTypeConfiguration<IdeaReaction>
{
    public void Configure(EntityTypeBuilder<IdeaReaction> builder)
    {
        #region Properties
        builder.HasIndex("IdeaId", "YouthToken", nameof(IdeaReaction.Emoji))
            .IsUnique();
        
        #endregion
        #region Relations
        builder.HasOne(ir => ir.Idea)
            .WithMany(i => i.Reactions)
            .HasForeignKey("IdeaId")
            .IsRequired();
        #endregion
    }
}
public class ResponseReactionConfig : IEntityTypeConfiguration<ResponseReaction>
{
    public void Configure(EntityTypeBuilder<ResponseReaction> builder)
    {
        #region Properties
        builder.Property<int>("ResponseId");

        builder.HasIndex("ResponseId", "YouthToken", nameof(ResponseReaction.Emoji))
            .IsUnique();
        #endregion

        #region Relations
        builder.HasOne(rr => rr.IdeaResponse)
            .WithMany(r => r.Reactions)
            .HasForeignKey("ResponseId")
            .IsRequired();
        #endregion
    }
}
