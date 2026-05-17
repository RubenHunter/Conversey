using Conversey.BL.Domain.Administration;

namespace Conversey.UI_MVC.Models.WorkspaceAdmin;

public class AdminWorkspaceDetailsViewModel
{
    public Workspace Workspace { get; set; }
    public IEnumerable<BL.Domain.Administration.WorkspaceAdmin> WorkspaceAdmins { get; set; }
}