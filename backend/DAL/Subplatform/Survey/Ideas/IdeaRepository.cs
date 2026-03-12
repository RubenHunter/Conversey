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
            .Include(i => i.Project)
            .Include(i => i.Responses)
            .FirstOrDefault(i => i.Id == ideaId)
            ?? throw new KeyNotFoundException($"Idea with id {ideaId} not found.");
    }

    public IReadOnlyCollection<Idea> ReadAllIdeas()
    {
        return _dbContext.Ideas
            .Include(i => i.Project)
            .Include(i => i.Responses)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Idea> ReadIdeasByProjectId(int projectId)
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

    public void DeleteIdea(int ideaId)
    {
        var idea = _dbContext.Ideas.Find(ideaId)
            ?? throw new KeyNotFoundException($"Idea with id {ideaId} not found.");
        _dbContext.Ideas.Remove(idea);
        _dbContext.SaveChanges();
    }

    public void CreateResponse(Response response)
    {
        _dbContext.Responses.Add(response);
        _dbContext.SaveChanges();
    }

    public Response ReadResponseById(int responseId)
    {
        return _dbContext.Responses
            .Include(r => r.Idea)
            .FirstOrDefault(r => r.Id == responseId)
            ?? throw new KeyNotFoundException($"Response with id {responseId} not found.");
    }

    public IReadOnlyCollection<Response> ReadResponsesByIdeaId(int ideaId)
    {
        return _dbContext.Responses
            .Where(r => r.Idea.Id == ideaId)
            .ToList().AsReadOnly();
    }

    public void UpdateResponse(Response response)
    {
        _dbContext.Responses.Update(response);
        _dbContext.SaveChanges();
    }

    public void DeleteResponse(int responseId)
    {
        var response = _dbContext.Responses.Find(responseId)
            ?? throw new KeyNotFoundException($"Response with id {responseId} not found.");
        _dbContext.Responses.Remove(response);
        _dbContext.SaveChanges();
    }
}
