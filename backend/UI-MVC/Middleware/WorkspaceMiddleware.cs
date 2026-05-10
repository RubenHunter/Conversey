using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Middleware;

public class WorkspaceMiddleware(WorkspaceContext workspaceContext, IWorkspaceRepository workspaceRepository) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string host = context.Request.Host.Host;
        var parts = host.Split('.');
        
        // Skip if it's the root domain (e.g., conversey.be or localhost)
        if (parts.Length <= 2 && !host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return next(context);
        }

        var subdomain = parts.First();
        // If the first part is the domain name itself (e.g. conversey.be), skip
        if (subdomain.Equals("conversey", StringComparison.OrdinalIgnoreCase))
        {
            return next(context);
        }

        workspaceContext.CurrentWorkspace = workspaceRepository.ReadWorkspaceById(Slug.FromName(subdomain));
        if (workspaceContext.CurrentWorkspace == null)
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/login", StringComparison.OrdinalIgnoreCase))
            {
                return next(context);
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return context.Response.WriteAsync($"Workspace not found for subdomain: {subdomain}. Check your database and Slug names.");
        }

        return next(context);
    }
}
