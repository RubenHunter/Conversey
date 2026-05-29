using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;
using Conversey.UI_MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Middleware;

public class WorkspaceMiddleware(WorkspaceContext workspaceContext, IWorkspaceRepository workspaceRepository) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var host = context.Request.Host.Host;
        var path = context.Request.Path.Value ?? "";

        // bypass voor statische bestanden en health check
        if (path.Equals("/health", StringComparison.OrdinalIgnoreCase) || 
            path.StartsWith("/health/", StringComparison.OrdinalIgnoreCase) ||
            path.Contains(".") || 
            path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) || 
            path.StartsWith("/Assets/", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var parts = host.Split('.');
        
        // Skip if it's the root domain (e.g., conversey.be or localhost or www.conversey.be)
        if (parts.Length <= 2 || host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) || host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var subdomain = parts.First();
        var workspace = workspaceRepository.ReadWorkspaceById(Slug.FromName(subdomain));

        if (workspace == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"Workspace '{subdomain}' niet gevonden.");
            return;
        }

        workspaceContext.CurrentWorkspace = workspace;
        await next(context);
    }
}
