using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            .Include(i => i.Reactions)
            .SingleOrDefault(i => i.Id == ideaId);
    }

    public Idea ReadIdeaByIdWithProject(int ideaId)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .SingleOrDefault(i => i.Id == ideaId);
    }

    public Idea ReadIdeaByIdWithResponses(int ideaId)
    {
        return _dbContext.Ideas
            .Include(i => i.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Reactions)
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

    public IReadOnlyCollection<Idea> ReadAllIdeas()
    {
        return _dbContext.Ideas.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadAllIdeasWithProject()
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadAllIdeasWithResponses()
    {
        return _dbContext.Ideas
            .Include(i => i.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Reactions)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadAllIdeasWithProjectAndResponses()
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Reactions)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasFromProjectByProjectId(Slug projectSlug)
    {
        return _dbContext.Ideas
            .Where(i => i.Project.Id == projectSlug)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasFromProjectByProjectIdWithResponses(Slug projectSlug)
    {
        return _dbContext.Ideas
            .Include(i => i.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Reactions)
            .Where(i => i.Project.Id == projectSlug)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasFromProjectByYouthToken(Slug projectSlug, Guid youthToken)
    {
        return _dbContext.Ideas
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .Where(i => i.Project.Id == projectSlug && i.Youth.Token == youthToken)
            .OrderByDescending(i => i.SubmissionDate)
            .ThenByDescending(i => i.Id)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasFromTopicByProjectSlugAndTopicId(Slug projectSlug, int topicId)
    {
        return _dbContext.Ideas
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .Where(i => i.Project.Id == projectSlug && i.Topic.Id == topicId && i.Status == ModerationStatus.Approved)
            .OrderByDescending(i => i.SubmissionDate)
            .ThenByDescending(i => i.Id)
            .ToList().AsReadOnly();
    }

    public void UpdateIdea(Idea idea)
    {
        _dbContext.Ideas.Update(idea);
        _dbContext.SaveChanges();
    }

    public bool DeleteIdea(int ideaId)
    {
        var idea = _dbContext.Ideas
            .SingleOrDefault(i => i.Id == ideaId);
        if (idea == null) return false;

        _dbContext.Ideas.Remove(idea);
        _dbContext.SaveChanges();
        return true;
    }

    public void CreateResponse(Response response)
    {
        _dbContext.Responses.Add(response);
        _dbContext.SaveChanges();
    }

    public Response ReadResponseById(int responseId)
    {
        return _dbContext.Responses
            .Include(r => r.Youth)
            .Include(r => r.Reactions)
            .SingleOrDefault(r => r.Id == responseId);
    }

    public Response ReadResponseByIdWithIdea(int responseId)
    {
        return _dbContext.Responses
            .Include(r => r.Idea)
            .ThenInclude(i => i.Project)
            .Include(r => r.Youth)
            .Include(r => r.Reactions)
            .SingleOrDefault(r => r.Id == responseId);
    }

    public IReadOnlyCollection<Response> ReadResponsesFromIdeaByIdeaId(int ideaId)
    {
        return _dbContext.Responses
            .Include(r => r.Youth)
            .Include(r => r.Reactions)
            .Where(r => r.Idea.Id == ideaId)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Response> ReadResponsesFromIdeaByIdeaIdWithIdea(int ideaId)
    {
        return _dbContext.Responses
            .Include(r => r.Idea)
            .Include(r => r.Youth)
            .Include(r => r.Reactions)
            .Where(r => r.Idea.Id == ideaId)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .ToList().AsReadOnly();
    }

    public void UpdateResponse(Response response)
    {
        _dbContext.Responses.Update(response);
        _dbContext.SaveChanges();
    }

    public bool DeleteResponse(int responseId)
    {
        var response = _dbContext.Responses
            .SingleOrDefault(r => r.Id == responseId);
        if (response == null) return false;

        _dbContext.Responses.Remove(response);
        _dbContext.SaveChanges();
        return true;
    }

    public void CreateIdeaReaction(IdeaReaction reaction)
    {
        _dbContext.IdeaReactions.Add(reaction);
        _dbContext.SaveChanges();
    }

    public IdeaReaction ReadIdeaReaction(int ideaId, Guid youthToken, string emoji)
    {
        return _dbContext.IdeaReactions
            .SingleOrDefault(ir => ir.Idea.Id == ideaId && ir.Youth.Token == youthToken && ir.Emoji == emoji);
    }

    public IReadOnlyCollection<IdeaReaction> ReadIdeaReactionsFromIdeaByIdeaId(int ideaId)
    {
        return _dbContext.IdeaReactions
            .Where(ir => ir.Idea.Id == ideaId)
            .OrderBy(ir => ir.Emoji)
            .ThenBy(ir => ir.Id)
            .ToList().AsReadOnly();
    }

    public bool DeleteIdeaReaction(int ideaId, Guid youthToken, string emoji)
    {
        var reaction = _dbContext.IdeaReactions
            .SingleOrDefault(ir => ir.Idea.Id == ideaId && ir.Youth.Token == youthToken && ir.Emoji == emoji);
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

    public ResponseReaction ReadResponseReaction(int responseId, Guid youthToken, string emoji)
    {
        return _dbContext.ResponseReactions
            .SingleOrDefault(rr => rr.Response.Id == responseId && rr.Youth.Token == youthToken && rr.Emoji == emoji);
    }

    public IReadOnlyCollection<ResponseReaction> ReadResponseReactionsFromResponseByResponseId(int responseId)
    {
        return _dbContext.ResponseReactions
            .Where(rr => rr.Response.Id == responseId)
            .OrderBy(rr => rr.Emoji)
            .ThenBy(rr => rr.Id)
            .ToList().AsReadOnly();
    }

    public bool DeleteResponseReaction(int responseId, Guid youthToken, string emoji)
    {
        var reaction = _dbContext.ResponseReactions
            .SingleOrDefault(rr => rr.Response.Id == responseId && rr.Youth.Token == youthToken && rr.Emoji == emoji);
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
            .HasForeignKey("IdeaId")
            .IsRequired();

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

         #endregion

         #region Relations

         builder.HasOne(r => r.Youth)
             .WithMany(y => y.Reactions)
             .HasForeignKey("YouthToken")
             .IsRequired();
        
         #endregion
     }
}

public class ResponseConfig : IEntityTypeConfiguration<Response>
{
    public void Configure(EntityTypeBuilder<Response> builder)
    {
        #region Properties

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Text)
            .IsRequired()
            .HasMaxLength(4000);
        
        builder.Property(r => r.ModerationInfo)
            .HasConversion(
                m => ModerationInfoSerializer.Serialize(m),
                b => ModerationInfoSerializer.Deserialize(b)
            );

        
        #endregion

        #region Relations

        builder.HasOne(r => r.Youth)
            .WithMany(y => y.Responses)
            .HasForeignKey("YouthToken")
            .IsRequired();

        builder.HasMany(r => r.Reactions)
            .WithOne(rr => rr.Response)
            .HasForeignKey("ResponseId")
            .IsRequired();
        #endregion
    }
}

public class IdeaReactionConfig : IEntityTypeConfiguration<IdeaReaction>
{
    public void Configure(EntityTypeBuilder<IdeaReaction> builder)
    {
        #region Properties
        // builder.HasIndex([ir => new { ir.Idea, ir.Youth, ir.Emoji }])
        //     .IsUnique();
        builder.HasIndex("IdeaId", "YouthToken", "Emoji")
            .IsUnique();
        #endregion
    }
}
public class ResponseReactionConfig : IEntityTypeConfiguration<ResponseReaction>
{
    public void Configure(EntityTypeBuilder<ResponseReaction> builder)
    {
        #region Properties
        // builder.HasIndex(rr => new { rr.Response, rr.Youth, rr.Emoji })
        //     .IsUnique();
        builder.HasIndex("ResponseId", "YouthToken", "Emoji")
            .IsUnique();
        #endregion
    }
}
