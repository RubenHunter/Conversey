using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IAdminRepository
{
    IReadOnlyCollection<WorkspaceAdmin> ReadAllWorkspaceAdminsByWorkspaceIdWithWorkspace(Slug id);
    IReadOnlyCollection<WorkspaceAdminUser> ReadAllWorkspaceAdmins();

    public void CreateWorkspaceAdmin(WorkspaceAdmin workspaceAdmin);
}