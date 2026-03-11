using Conversey.BL.Domain.Subplatform;

namespace Conversey.BL.Subplatform;

public interface IWorkspaceManager
{
    IReadOnlyCollection<Workspace> GetAllWorkspaces();
    Workspace CreateWorkspace(string name, string slug);
    Workspace GetWorkspaceBySlug(string slug);
    Workspace GetWorkspaceById(int id);
}