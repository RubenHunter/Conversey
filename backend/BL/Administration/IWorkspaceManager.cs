using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IWorkspaceManager
{
    IReadOnlyCollection<Workspace> GetAllWorkspaces();
    IReadOnlyCollection<Workspace> GetAllWorkspacesWithProjects();
    Workspace CreateWorkspace(string name, Slug slug);
    Workspace GetWorkspaceBySlug(Slug slug);
    Workspace GetWorkspaceBySlugWithProjects(Slug slug);
    Workspace GetWorkspaceById(Slug id);
    Workspace GetWorkspaceByIdWithProjects(Slug id);
}