using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;

namespace Conversey.BL.Subplatform;

public interface IWorkspaceManager
{
    IReadOnlyCollection<Workspace> GetAllWorkspaces();
    Workspace CreateWorkspace(string name, Slug slug);
    Workspace GetWorkspaceBySlug(Slug slug);
    Workspace GetWorkspaceById(int id);
}