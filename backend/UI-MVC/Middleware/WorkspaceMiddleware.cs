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
        if (path.StartsWith("/health") || 
            path.Contains(".") || 
            path.StartsWith("/lib/") || 
            path.StartsWith("/Assets/"))
        {
            await next(context);
            return;
        }

        var parts = host.Split('.');
        
        // Skip if it's the root domain (e.g., conversey.be or localhost)
        if (parts.Length <= 2 && !host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var subdomain = parts.First();
        // If the first part is the domain name itself (e.g. conversey.be), skip
        if (subdomain.Equals("conversey", StringComparison.OrdinalIgnoreCase))
        {
            return next(context);
        }

        try 
        {
            workspaceContext.CurrentWorkspace = workspaceRepository.ReadWorkspaceById(Slug.FromName(subdomain));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error reading workspace for {subdomain}: {ex.Message}");
            // Redirect to root domain if we can't find the workspace
            context.Response.Redirect("https://conversey.be/login");
            return Task.CompletedTask;
        }

        if (workspaceContext.CurrentWorkspace == null)
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/login", StringComparison.OrdinalIgnoreCase))
            {
                return next(context);
            }

            // Redirect back to root portal if subdomain is invalid
            context.Response.Redirect("https://conversey.be/login");
            return Task.CompletedTask;
        }

        return next(context);
    }
}
