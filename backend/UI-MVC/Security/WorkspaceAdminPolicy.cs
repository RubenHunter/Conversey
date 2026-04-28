using Conversey.DAL;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Security;

public static class WorkspaceAdminPolicy
{
    public const string Name = "WorkspaceAdminPolicy";
}

public sealed class WorkspaceAdminRequirement : IAuthorizationRequirement;


public sealed class WorkspaceAdminHandler(
    WorkspaceContext workspaceContext,
    UserManager<IdentityUser> userManager)
    : AuthorizationHandler<WorkspaceAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceAdminRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true || workspaceContext.CurrentWorkspace == null)
        {
            return;
        }

        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return;
        }

        // Check if user is a WorkspaceAdmin (has WorkspaceId property)
        if (user is WorkspaceAdminUser workspaceAdmin)
        {
            if (workspaceAdmin.Workspace.Id == workspaceContext.CurrentWorkspace.Id)
            {
                context.Succeed(requirement);
            }
        }
        
    }
}
