using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IAdminRepository
{
    IReadOnlyCollection<WorkspaceAdmin> ReadAllWorkspaceAdminsByWorkspaceIdWithWorkspace(Slug id);
    IReadOnlyCollection<WorkspaceAdmin> ReadAllWorkspaceAdmins();

    Task<WorkspaceAdmin> ReadWorkspaceAdminById(Guid id);
    Task CreateWorkspaceAdmin(WorkspaceAdmin workspaceAdmin, string tempPassword);
    Task SetWorkspaceAdminFirstLogin(Guid workspaceAdminId, bool isFirstLogin);
    Task UpdateWorkspaceAdmin(WorkspaceAdmin workspaceAdmin);
    Task DeleteWorkspaceAdmin(Guid workspaceAdminId);
    Task<(bool EmailExists, bool UsernameExists)> CheckWorkspaceAdminConflicts(Slug workspaceId, string email, string username, Guid? excludeWorkspaceAdminId = null);

    IReadOnlyCollection<ConverseyAdmin> ReadAllConverseyAdmins();
    Task CreateConverseyAdmin(ConverseyAdmin converseyAdmin, string tempPassword);
    Task SetConverseyAdminFirstLogin(Guid converseyAdminId, bool isFirstLogin);
    Task UpdateConverseyAdmin(ConverseyAdmin converseyAdmin);
    Task DeleteConverseyAdmin(Guid converseyAdminId);
    Task<(bool EmailExists, bool UsernameExists)> CheckConverseyAdminConflicts(string email, string username, Guid? excludeConverseyAdminId = null);
}
