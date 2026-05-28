using Microsoft.AspNetCore.Authorization;

namespace Conversey.UI_MVC.Security;

public static class ConverseyAdminPolicy
{
    public const string Name = "ConverseyAdminPolicy";
}

public sealed class ConverseyAdminRequirement : IAuthorizationRequirement;

public sealed class ConverseyAdminHandler(
    IAdminAccessService adminAccessService)
    : AuthorizationHandler<ConverseyAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConverseyAdminRequirement requirement)
    {
        if (await adminAccessService.IsConverseyAdminAsync(context.User))
        {
            context.Succeed(requirement);
        }
    }
}
