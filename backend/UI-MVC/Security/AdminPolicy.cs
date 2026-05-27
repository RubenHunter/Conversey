using Microsoft.AspNetCore.Authorization;

namespace Conversey.UI_MVC.Security;

public static class AdminPolicy
{
    public const string Name = "AdminPolicy";
}

public sealed class AdminRequirement : IAuthorizationRequirement;

public sealed class AdminHandler(
    IAdminAccessService adminAccessService)
    : AuthorizationHandler<AdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminRequirement requirement)
    {
        if (await adminAccessService.IsConverseyAdminAsync(context.User) ||
            await adminAccessService.IsWorkspaceAdminForCurrentWorkspaceAsync(context.User))
        {
            context.Succeed(requirement);
        }
    }
}
