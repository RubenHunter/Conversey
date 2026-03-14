using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;

namespace Conversey.DAL.Subplatform;

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