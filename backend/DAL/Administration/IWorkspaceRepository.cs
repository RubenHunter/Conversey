using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IWorkspaceRepository
{
    IReadOnlyCollection<Workspace> ReadAllWorkspaces();
    Workspace ReadWorkspaceBySlug(Slug slug);
    Workspace ReadWorkspaceById(Slug id);
    void CreateWorkspace(Workspace workspace);
    void UpdateWorkspace(Workspace updatedWorkspace);
    void DeleteWorkspace(Workspace workspace);
}