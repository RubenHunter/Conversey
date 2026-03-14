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
            .SingleOrDefault(i => i.Id == ideaId);
    }

    public Idea ReadIdeaByIdWithProject(int ideaId)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .SingleOrDefault(i => i.Id == ideaId);
    }

    public Idea ReadIdeaByIdWithResponses(int ideaId)
    {
        return _dbContext.Ideas
            .Include(i => i.Responses)
            .SingleOrDefault(i => i.Id == ideaId);
    }

    public Idea ReadIdeaByIdWithProjectAndResponses(int ideaId)
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Responses)
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
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadAllIdeasWithResponses()
    {
        return _dbContext.Ideas
            .Include(i => i.Responses)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadAllIdeasWithProjectAndResponses()
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Responses)
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
            .Include(i => i.Responses)
            .Where(i => i.Project.Id == projectId)
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
            .SingleOrDefault(r => r.Id == responseId);
    }

    public Response ReadResponseByIdWithIdea(int responseId)
    {
        return _dbContext.Responses
            .Include(r => r.Idea)
            .SingleOrDefault(r => r.Id == responseId);
    }

    public IReadOnlyCollection<Response> ReadResponsesFromIdeaByIdeaId(int ideaId)
    {
        return _dbContext.Responses
            .Where(r => r.Idea.Id == ideaId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Response> ReadResponsesFromIdeaByIdeaIdWithIdea(int ideaId)
    {
        return _dbContext.Responses
            .Include(r => r.Idea)
            .Where(r => r.Idea.Id == ideaId)
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
}
