using Conversey.BL.Domain.Entities.Identity;

namespace Conversey.DAL.EF;

public class Repository : IRepository
{
    private readonly ConverseyDbContext _context;

    public Repository(ConverseyDbContext context)
    {
        _context = context;
    }


    public IReadOnlyCollection<Workspace> ReadAllWorkspaces()
    {
        return _context.Workspaces.ToList();
    }
}