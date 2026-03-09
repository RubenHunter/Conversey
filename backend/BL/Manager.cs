using Conversey.BL.Domain.Entities.Identity;
using Conversey.DAL;

namespace Conversey.BL;

public class Manager : IManager
{
    private IWorkspaceRepository _workspaceRepository;

    public Manager(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public IReadOnlyCollection<Workspace> GetAllWorkspaces()
    {
        return _workspaceRepository.ReadAllWorkspaces();
    }
}