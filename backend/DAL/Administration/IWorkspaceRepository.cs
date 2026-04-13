using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IWorkspaceRepository
{
    IReadOnlyCollection<Workspace> ReadAllWorkspaces();
    IReadOnlyCollection<Workspace> ReadAllWorkspacesWithProjects();
    Workspace ReadWorkspaceBySlug(Slug slug);
    Workspace ReadWorkspaceBySlugWithProjects(Slug slug);
    Workspace ReadWorkspaceById(int id);
    Workspace ReadWorkspaceByIdWithProjects(int id);
    void CreateWorkspace(Workspace workspace);
}