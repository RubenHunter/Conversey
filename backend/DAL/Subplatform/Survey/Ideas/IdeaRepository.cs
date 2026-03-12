using Conversey.BL.Domain.Subplatform.Survey.Ideation;

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
}