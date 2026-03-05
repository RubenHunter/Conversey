using Conversey.BL.Domain.Entities.Identity;

namespace Conversey.BL.Services;

public class WorkspaceService
{
    private static readonly List<Workspace> Workspaces = [];

    public Workspace CreateWorkspace(string name)
    {
        var workspace = new Workspace(name, SlugExists);
        
        Workspaces.Add(workspace);

        return workspace;
    }

    public Workspace GetBySlug(string slug)
    {
        return Workspaces.FirstOrDefault(w => w.Slug == slug);
    }

    public IEnumerable<Workspace> GetWorkspaces()
    {
        return Workspaces;
    }
    
    private bool SlugExists(string slug)
    {
        return Workspaces.Any(w => w.Slug == slug);
    }
}