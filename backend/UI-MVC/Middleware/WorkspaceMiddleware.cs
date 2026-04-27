using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;
using Conversey.UI_MVC.Models;

namespace Conversey.UI_MVC.Middleware;

public class WorkspaceMiddleware(WorkspaceContext workspaceContext, IWorkspaceRepository workspaceRepository) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var subdomain = context.Request.Host.Host.Split('.').First();
        
        // Skip workspace check for localhost in development
        if (subdomain == "localhost" || context.Request.Host.Host == "127.0.0.1")
        {
            workspaceContext.CurrentWorkspace = null;
            return next(context);
        }
        
        workspaceContext.CurrentWorkspace = workspaceRepository.ReadWorkspaceById(Slug.FromName(subdomain));
        if (workspaceContext.CurrentWorkspace == null)
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/Identity", StringComparison.OrdinalIgnoreCase))
            {
                return next(context);
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }

        return next(context);
    }
}
