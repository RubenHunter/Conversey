using Conversey.BL.Domain.Administration;

namespace Conversey.UI_MVC.Models.AdminManagement;

public class AdminManagementViewModel
{
    public IEnumerable<ConverseyAdmin> ConverseyAdmins { get; set; } = [];
    public IEnumerable<IGrouping<Workspace, BL.Domain.Administration.WorkspaceAdmin>> WorkspaceAdminsByWorkspace { get; set; } = [];
}
