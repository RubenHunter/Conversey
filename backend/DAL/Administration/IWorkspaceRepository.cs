using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IWorkspaceRepository
{
    IReadOnlyCollection<Workspace> ReadAllWorkspaces();
    IReadOnlyCollection<Workspace> ReadAllWorkspacesWithProjects();
    Workspace ReadWorkspaceBySlug(Slug slug);
    Workspace ReadWorkspaceBySlugWithProjects(Slug slug);
    Workspace ReadWorkspaceById(Slug id);
    Workspace ReadWorkspaceByIdWithProjects(Slug id);
    void CreateWorkspace(Workspace workspace);
}