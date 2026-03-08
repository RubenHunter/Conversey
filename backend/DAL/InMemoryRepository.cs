using Conversey.BL.Domain.Entities.Identity;

namespace Conversey.DAL;

public class InMemoryRepository : IRepository
{
    
    private readonly ICollection<Workspace> _workspaces;

    public InMemoryRepository()
    {
        _workspaces = [
            new Workspace
            {
                Id = 1,
                Name = "Gemeente"
            },
            new Workspace
            {
                Id = 2,
                Name = "School"
            }
        ];
    }

    public IReadOnlyCollection<Workspace> ReadAllWorkspaces()
    {
        return _workspaces.ToList().AsReadOnly();
    }
}