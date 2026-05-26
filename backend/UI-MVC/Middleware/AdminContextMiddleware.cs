using Conversey.DAL;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Middleware;

public class AdminContextMiddleware(AdminContext adminContext, UserManager<IdentityUser> userManager) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is AdminUser adminUser)
            {
                adminContext.CurrentAdmin = AdminContext.ToDomain(adminUser);
            }
        }

        await next(context);
    }
}
