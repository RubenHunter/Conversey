using Conversey.BL.Domain.Entities.Identity;

namespace Conversey.BL;

public interface IManager
{
    IReadOnlyCollection<Domain.Workspace.Workspace> GetAllWorkspaces();
}