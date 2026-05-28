using System.Security.Claims;
using Conversey.DAL;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Security;

public interface IAdminAccessService
{
    Task<bool> IsConverseyAdminAsync(ClaimsPrincipal user);
    Task<bool> IsWorkspaceAdminForCurrentWorkspaceAsync(ClaimsPrincipal user);
}

public sealed class AdminAccessService(
    WorkspaceContext workspaceContext,
    UserManager<IdentityUser> userManager)
    : IAdminAccessService
{
    public async Task<bool> IsConverseyAdminAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var identityUser = await userManager.GetUserAsync(user);
        return identityUser is ConverseyAdminUser;
    }

    public async Task<bool> IsWorkspaceAdminForCurrentWorkspaceAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true || workspaceContext.CurrentWorkspace == null)
        {
            return false;
        }

        var identityUser = await userManager.GetUserAsync(user);
        return identityUser is WorkspaceAdminUser workspaceAdmin &&
               workspaceAdmin.Workspace.Id == workspaceContext.CurrentWorkspace.Id;
    }
}
