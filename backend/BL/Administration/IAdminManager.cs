using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IAdminManager
{
    Task<WorkspaceAdmin> GetWorkspaceAdminById(Guid id);
    IEnumerable<WorkspaceAdmin> GetAllWorkspaceAdminsByWorkspaceIdWithWorkspace(Slug id);
    IEnumerable<WorkspaceAdmin> GetAllWorkspaceAdmins();
    Task<(WorkspaceAdmin Admin, string OneTimePassword)> AddWorkspaceAdmin(string email, string username, string phoneNumber, Slug workspaceId);
    Task SetWorkspaceAdminFirstLogin(Guid workspaceAdminId, bool isFirstLogin);
    Task EditWorkspaceAdmin(WorkspaceAdmin workspaceAdmin);
    Task RemoveWorkspaceAdmin(Guid workspaceAdminId);

    IEnumerable<ConverseyAdmin> GetAllConverseyAdmins();
    Task<(ConverseyAdmin Admin, string OneTimePassword)> AddConverseyAdmin(string email, string username, string phoneNumber);
    Task SetConverseyAdminFirstLogin(Guid converseyAdminId, bool isFirstLogin);
    Task EditConverseyAdmin(ConverseyAdmin converseyAdmin);
    Task RemoveConverseyAdmin(Guid converseyAdminId);
}
