using Conversey.BL.Domain.Subplatform;

namespace Conversey.DAL.Subplatform;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly ConverseyDbContext _context;

    public WorkspaceRepository(ConverseyDbContext context)
    {
        _context = context;
    }


    public IReadOnlyCollection<Workspace> ReadAllWorkspaces()
    {
        return _context.Workspaces.ToList().AsReadOnly();
    }
}