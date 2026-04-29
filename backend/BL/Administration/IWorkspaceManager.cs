using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IWorkspaceManager
{
    IEnumerable<Workspace> GetAllWorkspaces();
    Workspace AddWorkspace(string name);
    Workspace GetWorkspaceById(Slug id);
    void EditWorkspace(Workspace updatedWorkspace);
    void RemoveWorkspace(Slug id);
}