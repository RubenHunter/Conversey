using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Survey.Ideas;

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

    public IReadOnlyCollection<Idea> ReadIdeasFromProjectByProjectId(int projectId)
    {
        return _dbContext.Ideas
            .Where(i => i.Project.Id == projectId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasFromProjectByProjectIdWithResponses(int projectId)
    {
        return _dbContext.Ideas
            .Include(i => i.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Youth)
            .Include(i => i.Responses)
            .ThenInclude(r => r.Reactions)
            .Where(i => i.Project.Id == projectId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasFromProjectByYouthToken(int projectId, string youthToken)
    {
        return _dbContext.Ideas
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .Where(i => i.Project.Id == projectId && i.Youth.Token == youthToken)
            .OrderByDescending(i => i.SubmissionDate)
            .ThenByDescending(i => i.Id)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasFromTopicByProjectSlugAndTopicId(Slug projectSlug, int topicId)
    {
        return _dbContext.Ideas
            .Include(i => i.Youth)
            .Include(i => i.Reactions)
            .Where(i => i.Project.Slug == projectSlug && i.Topic.Id == topicId)
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

    public IdeaReaction ReadIdeaReaction(int ideaId, string youthToken, string emoji)
    {
        return _dbContext.IdeaReactions
            .SingleOrDefault(ir => ir.IdeaId == ideaId && ir.YouthToken == youthToken && ir.Emoji == emoji);
    }

    public IReadOnlyCollection<IdeaReaction> ReadIdeaReactionsFromIdeaByIdeaId(int ideaId)
    {
        return _dbContext.IdeaReactions
            .Where(ir => ir.IdeaId == ideaId)
            .OrderBy(ir => ir.Emoji)
            .ThenBy(ir => ir.Id)
            .ToList().AsReadOnly();
    }

    public bool DeleteIdeaReaction(int ideaId, string youthToken, string emoji)
    {
        var reaction = _dbContext.IdeaReactions
            .SingleOrDefault(ir => ir.IdeaId == ideaId && ir.YouthToken == youthToken && ir.Emoji == emoji);
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

    public ResponseReaction ReadResponseReaction(int responseId, string youthToken, string emoji)
    {
        return _dbContext.ResponseReactions
            .SingleOrDefault(rr => rr.ResponseId == responseId && rr.YouthToken == youthToken && rr.Emoji == emoji);
    }

    public IReadOnlyCollection<ResponseReaction> ReadResponseReactionsFromResponseByResponseId(int responseId)
    {
        return _dbContext.ResponseReactions
            .Where(rr => rr.ResponseId == responseId)
            .OrderBy(rr => rr.Emoji)
            .ThenBy(rr => rr.Id)
            .ToList().AsReadOnly();
    }

    public bool DeleteResponseReaction(int responseId, string youthToken, string emoji)
    {
        var reaction = _dbContext.ResponseReactions
            .SingleOrDefault(rr => rr.ResponseId == responseId && rr.YouthToken == youthToken && rr.Emoji == emoji);
        if (reaction == null) return false;

        _dbContext.ResponseReactions.Remove(reaction);
        _dbContext.SaveChanges();
        return true;
    }
}
