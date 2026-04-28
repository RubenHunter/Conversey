using Conversey.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Security;

public static class ConverseyAdminPolicy
{
    public const string Name = "ConverseyAdminPolicy";
}

public sealed class ConverseyAdminRequirement : IAuthorizationRequirement;

public sealed class ConverseyAdminHandler(
    UserManager<IdentityUser> userManager)
    : AuthorizationHandler<ConverseyAdminRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConverseyAdminRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var user = await userManager.GetUserAsync(context.User);
        if (user is ConverseyAdminUser)
        {
            context.Succeed(requirement);
        }
    }
}