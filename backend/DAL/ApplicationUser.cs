using Conversey.BL.Domain.Administration;
using Microsoft.AspNetCore.Identity;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL;

public class ConverseyAdminUser : AdminUser
{
    // Conversey-specific properties
}

public class WorkspaceAdminUser : AdminUser
{
    public Workspace Workspace { get; set; }
}

public class AdminUser : IdentityUser;