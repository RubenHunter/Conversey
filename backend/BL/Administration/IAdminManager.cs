using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IAdminManager
{
    Task<WorkspaceAdmin> GetWorkspaceAdminById(Guid id);
    IEnumerable<WorkspaceAdmin> GetAllWorkspaceAdminsByWorkspaceIdWithWorkspace(Slug id);
    Task<WorkspaceAdmin> AddWorkspaceAdmin(string email, string username, string phoneNumber, Slug workspaceId);
    Task EditWorkspaceAdmin(WorkspaceAdmin workspaceAdmin);
    Task RemoveWorkspaceAdmin(Guid workspaceAdminId);
}
