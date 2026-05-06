using Conversey.BL.Domain.Administration;
using Conversey.DAL;

namespace Conversey.UI_MVC.Models;

public class AdminContext
{
    public BL.Domain.Administration.Admin CurrentAdmin { get; set; }

    public static BL.Domain.Administration.Admin ToDomain(AdminUser adminUser)
    {
        if (adminUser is WorkspaceAdminUser workspaceAdminUser)
        {
            return new BL.Domain.Administration.WorkspaceAdmin
            {
                Id = Guid.Parse(workspaceAdminUser.Id),
                Email = workspaceAdminUser.Email,
                Workspace = workspaceAdminUser.Workspace
            };
        }

        return new ConverseyAdmin
        {
            Id = Guid.Parse(adminUser.Id),
            Email = adminUser.Email
        };
    }
}