using Conversey.BL.Domain.Subplatform;
using Conversey.DAL.Subplatform;

namespace Conversey.BL.Subplatform;

public class WorkspaceManager: IWorkspaceManager
{
    private IWorkspaceRepository _workspaceRepository;

    public WorkspaceManager(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public IReadOnlyCollection<Workspace> GetAllWorkspaces()
    {
        return _workspaceRepository.ReadAllWorkspaces();
    }
}