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

    public Workspace ReadWorkspaceBySlug(string slug)
    {
        return _context.Workspaces.FirstOrDefault(w => w.Slug == slug) ?? throw new KeyNotFoundException($"Workspace with slug: {slug} not found");
    }
    
    public Workspace ReadWorkspaceById(int id)
    {
        return _context.Workspaces.FirstOrDefault(w => w.Id == id) ?? throw new KeyNotFoundException($"Workspace with id: {id} not found");
    }

    public void CreateWorkspace(Workspace workspace)
    {
        _context.Workspaces.Add(workspace);
        _context.SaveChanges();
    }
}