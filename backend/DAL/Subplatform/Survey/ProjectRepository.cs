using Conversey.BL.Domain.Subplatform.Survey;

namespace Conversey.DAL.Subplatform.Survey;

public class ProjectRepository : IProjectRepository
{
    
    private readonly ConverseyDbContext _dbContext;

    public ProjectRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Project ReadProjectById(int projectId)
    {
        return _dbContext.Projects.Single(p => p.Id == projectId);
    }
}