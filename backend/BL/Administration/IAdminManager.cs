using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IAdminManager
{
    IEnumerable<WorkspaceAdmin> GetAllWorkspaceAdminsByWorkspaceIdWithWorkspace(Slug id);
    WorkspaceAdmin AddWorkspaceAdmin(string email, Slug workspaceId);
}