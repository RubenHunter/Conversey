using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;
using Microsoft.EntityFrameworkCore;

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

    public IReadOnlyCollection<Workspace> ReadAllWorkspacesWithProjects()
    {
        return _context.Workspaces
            .Include(w => w.Projects)
            .ToList().AsReadOnly();
    }

    public Workspace ReadWorkspaceBySlug(Slug slug)
    {
        return _context.Workspaces.SingleOrDefault(w => w.Slug == slug);
    }

    public Workspace ReadWorkspaceBySlugWithProjects(Slug slug)
    {
        return _context.Workspaces
            .Include(w => w.Projects)
            .SingleOrDefault(w => w.Slug == slug);
    }

    public Workspace ReadWorkspaceById(int id)
    {
        return _context.Workspaces.SingleOrDefault(w => w.Id == id);
    }

    public Workspace ReadWorkspaceByIdWithProjects(int id)
    {
        return _context.Workspaces
            .Include(w => w.Projects)
            .SingleOrDefault(w => w.Id == id);
    }

    public void CreateWorkspace(Workspace workspace)
    {
        _context.Workspaces.Add(workspace);
        _context.SaveChanges();
    }
}