using Microsoft.AspNetCore.Identity;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL;

public class ApplicationUser : IdentityUser
{
    public Slug WorkspaceId { get; set; }
}
