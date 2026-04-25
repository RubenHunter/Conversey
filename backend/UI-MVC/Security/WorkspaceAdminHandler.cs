using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Conversey.DAL;

namespace Conversey.UI_MVC.Security;

public sealed class WorkspaceAdminHandler(
    WorkspaceContext workspaceContext,
    UserManager<ApplicationUser> userManager)
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
        if (user == null || string.IsNullOrWhiteSpace(user.WorkspaceId.Text))
        {
            return;
        }

        if (user.WorkspaceId == workspaceContext.CurrentWorkspace.Id)
        {
            context.Succeed(requirement);
        }
    }
}
