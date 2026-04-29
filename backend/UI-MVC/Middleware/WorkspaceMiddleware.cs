using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Middleware;

public class WorkspaceMiddleware(WorkspaceContext workspaceContext, IWorkspaceRepository workspaceRepository, UserManager<IdentityUser> userManager) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        bool hasSubdomain = context.Request.Host.Host.Contains('.');
        if (!hasSubdomain)
        {
            return next(context);
        }
        
        var subdomain = context.Request.Host.Host.Split('.').First();

        workspaceContext.CurrentWorkspace = workspaceRepository.ReadWorkspaceById(Slug.FromName(subdomain));
        if (workspaceContext.CurrentWorkspace == null)
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/login", StringComparison.OrdinalIgnoreCase))
            {
                return next(context);
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }

        return next(context);
    }
}
