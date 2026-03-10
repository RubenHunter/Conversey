using Conversey.BL.Domain.Subplatform;

namespace Conversey.BL.Subplatform;

public interface IWorkspaceManager
{
    IReadOnlyCollection<Workspace> GetAllWorkspaces();
}