using Conversey.BL.Domain.Administration;
using Conversey.DAL;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Models.Admin;

public class AdminWorkspaceDetailsViewModel
{
    public Workspace Workspace { get; set; }
    public IEnumerable<WorkspaceAdmin> WorkspaceAdmins { get; set; }
}