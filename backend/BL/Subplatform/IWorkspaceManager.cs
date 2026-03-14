using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;

namespace Conversey.BL.Subplatform;

public interface IWorkspaceManager
{
    IReadOnlyCollection<Workspace> GetAllWorkspaces();
    IReadOnlyCollection<Workspace> GetAllWorkspacesWithProjects();
    Workspace CreateWorkspace(string name, string slug);
    Workspace GetWorkspaceBySlug(Slug slug);
    Workspace GetWorkspaceBySlugWithProjects(Slug slug);
    Workspace GetWorkspaceById(int id);
    Workspace GetWorkspaceByIdWithProjects(int id);
}