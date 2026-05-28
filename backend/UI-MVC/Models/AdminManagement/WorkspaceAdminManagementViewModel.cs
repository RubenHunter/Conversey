using Adm = Conversey.BL.Domain.Administration.WorkspaceAdmin;

namespace Conversey.UI_MVC.Models.AdminManagement;

public class WorkspaceAdminManagementViewModel
{
    public List<Adm> Admins { get; set; } = [];
    public string WorkspaceName { get; set; } = string.Empty;
    public string WorkspaceId { get; set; } = string.Empty;
}
