using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IWorkspaceManager
{
    IEnumerable<Workspace> GetAllWorkspaces();
    Workspace CreateWorkspace(string name);
    Workspace GetWorkspaceById(Slug id);
}