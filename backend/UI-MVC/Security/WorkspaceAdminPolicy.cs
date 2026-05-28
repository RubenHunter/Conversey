using Microsoft.AspNetCore.Authorization;

namespace Conversey.UI_MVC.Security;

public static class WorkspaceAdminPolicy
{
    public const string Name = "WorkspaceAdminPolicy";
}

public sealed class WorkspaceAdminRequirement : IAuthorizationRequirement;

public sealed class WorkspaceAdminHandler(
    IAdminAccessService adminAccessService)
    : AuthorizationHandler<WorkspaceAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceAdminRequirement requirement)
    {
        if (await adminAccessService.IsWorkspaceAdminForCurrentWorkspaceAsync(context.User))
        {
            context.Succeed(requirement);
        }
    }
}
