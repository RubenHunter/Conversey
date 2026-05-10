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
        
        // Skip if it's the root domain (e.g., conversey.be or localhost or www.conversey.be)
        if (parts.Length <= 2 || host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) || host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var subdomain = parts.First();
        var workspace = workspaceRepository.GetWorkspaceWithProjects(subdomain);

        if (workspace == null)
        {
            // Geen loop meer, maar een duidelijke melding
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync($"Workspace '{subdomain}' not found.");
            return;
        }

        try 
        {
            workspaceContext.CurrentWorkspace = workspaceRepository.ReadWorkspaceById(Slug.FromName(subdomain));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error reading workspace for {subdomain}: {ex.Message}");
            context.Response.Redirect("https://conversey.be/login");
            return;
        }

        if (workspaceContext.CurrentWorkspace == null)
        {
            if (context.Request.Path.StartsWithSegments("/login", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            context.Response.Redirect("https://conversey.be/login");
            return;
        }

        await next(context);
    }
}
