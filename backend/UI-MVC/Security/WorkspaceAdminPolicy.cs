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
        try 
        {
            logger.LogInformation("--- WorkspaceAdminHandler started for User: {UserIdentity} ---", context.User.Identity?.Name);

            if (context.User.Identity?.IsAuthenticated != true)
            {
                logger.LogWarning("User is not authenticated.");
                return;
            }

            if (workspaceContext.CurrentWorkspace == null)
            {
                logger.LogWarning("CurrentWorkspace is null. Access denied for workspace-specific area.");
                return;
            }

            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                logger.LogError("UserManager.GetUserAsync returned null for authenticated user {UserName}.", context.User.Identity?.Name);
                return;
            }

            var userWorkspaceId = user.WorkspaceId.Text ?? "NULL";
            var currentWorkspaceId = workspaceContext.CurrentWorkspace.Id.Text ?? "NULL";

            logger.LogInformation("Comparing Workspace IDs - User: {UserWS}, Current: {CurrentWS}", userWorkspaceId, currentWorkspaceId);

            if (user.WorkspaceId == workspaceContext.CurrentWorkspace.Id)
            {
                logger.LogInformation("Access granted: IDs match.");
                context.Succeed(requirement);
            }
            else
            {
                logger.LogWarning("Access denied: User WorkspaceId '{UserWS}' does not match Current WorkspaceId '{CurrentWS}'.", userWorkspaceId, currentWorkspaceId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CRITICAL ERROR in WorkspaceAdminHandler: {Message}", ex.Message);
            // We don't rethrow here to prevent a 500, but the requirement won't be met (access denied)
        }
    }
}
