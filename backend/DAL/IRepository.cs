using Conversey.BL.Domain.Entities.Identity;

namespace Conversey.DAL;

public interface IRepository
{
    IReadOnlyCollection<Workspace> ReadAllWorkspaces();
}