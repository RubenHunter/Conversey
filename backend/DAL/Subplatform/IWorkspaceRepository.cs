using Conversey.BL.Domain.Subplatform;

namespace Conversey.DAL.Subplatform;

public interface IWorkspaceRepository
{
    IReadOnlyCollection<Workspace> ReadAllWorkspaces();
    Workspace ReadWorkspaceBySlug(string slug);
    Workspace ReadWorkspaceById(int id);
    void CreateWorkspace(Workspace workspace);
}