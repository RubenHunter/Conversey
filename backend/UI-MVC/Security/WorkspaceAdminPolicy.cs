using Conversey.DAL;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Conversey.UI_MVC.Security;

public static class WorkspaceAdminPolicy
{
    public const string Name = "WorkspaceAdminPolicy";
}

public sealed class WorkspaceAdminRequirement : IAuthorizationRequirement;


public sealed class WorkspaceAdminHandler(
    WorkspaceContext workspaceContext,
    UserManager<ApplicationUser> userManager,
    ILogger<WorkspaceAdminHandler> logger)
    : AuthorizationHandler<WorkspaceAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceAdminRequirement requirement)
    {
        logger.LogInformation("--- WorkspaceAdminHandler started ---");

        if (context.User.Identity?.IsAuthenticated != true)
        {
            logger.LogWarning("User is not authenticated.");
            return;
        }

        if (workspaceContext.CurrentWorkspace == null)
        {
            logger.LogWarning("CurrentWorkspace is null in WorkspaceContext.");
            return;
        }

        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            logger.LogError("UserManager returned null for the current user.");
            return;
        }

        logger.LogInformation("Checking access for User: {UserEmail}, UserWorkspaceId: {UserWorkspaceId}, CurrentWorkspaceId: {CurrentWorkspaceId}", 
            user.Email, user.WorkspaceId.Text, workspaceContext.CurrentWorkspace.Id.Text);

        // Check if user is a WorkspaceAdmin for the CURRENT workspace
        if (user.WorkspaceId == workspaceContext.CurrentWorkspace.Id)
        {
            logger.LogInformation("Access granted for workspace admin.");
            context.Succeed(requirement);
        }
        else
        {
            logger.LogWarning("Access denied: WorkspaceId mismatch.");
        }
    }
}
