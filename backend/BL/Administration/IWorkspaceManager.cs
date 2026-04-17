using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IWorkspaceManager
{
    IEnumerable<Workspace> GetAllWorkspaces();
    Workspace CreateWorkspace(string name, Slug slug);
    Workspace GetWorkspaceBySlug(Slug slug);
    Workspace GetWorkspace(Slug workspaceId);
}