using Conversey.BL.Domain.Entities.Identity;
using Conversey.DAL;

namespace Conversey.BL;

public class Manager : IManager
{
    private IRepository _repository;

    public Manager(IRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyCollection<Workspace> GetAllWorkspaces()
    {
        return _repository.ReadAllWorkspaces();
    }
}