using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;
using Conversey.UI_MVC.Models;

namespace Conversey.UI_MVC.Middleware;

public class WorkspaceMiddleware(WorkspaceContext workspaceContext, IWorkspaceRepository workspaceRepository) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var subdomain = context.Request.Host.Host.Split('.').First();
        workspaceContext.CurrentWorkspace = workspaceRepository.ReadWorkspaceById(Slug.FromName(subdomain));
        if (workspaceContext.CurrentWorkspace == null)
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/Identity", StringComparison.OrdinalIgnoreCase))
            {
                return next(context);
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return context.Response.WriteAsync($"Workspace not found for subdomain: {subdomain}. Check your database and Slug names.");
        }

        return next(context);
    }
}
