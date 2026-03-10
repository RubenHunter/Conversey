using Conversey.BL.Domain.Subplatform;

namespace Conversey.DAL.Subplatform;

public interface IWorkspaceRepository
{
    IReadOnlyCollection<Workspace> ReadAllWorkspaces();
}