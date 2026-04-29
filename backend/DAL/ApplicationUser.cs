using Conversey.BL.Domain.Administration;
using Microsoft.AspNetCore.Identity;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL;

public class ConverseyAdminUser : IdentityUser
{
    // Conversey-specific properties
}

public class WorkspaceAdminUser : IdentityUser
{
    public Workspace Workspace { get; set; }
}

public class ApplicationUser : IdentityUser
{
    public Slug WorkspaceId { get; set; }
}
