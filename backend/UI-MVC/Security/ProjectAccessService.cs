using System.Security.Claims;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Microsoft.AspNetCore.Authorization;

namespace Conversey.UI_MVC.Security;

public interface IProjectAccessService
{
    Task<bool> IsActiveProjectOrAdminAsync(Slug workspaceId, Slug projectId, ClaimsPrincipal user);
}

public sealed class ProjectAccessService(IProjectManager projectManager, IAuthorizationService authorizationService)
    : IProjectAccessService
{
    public async Task<bool> IsActiveProjectOrAdminAsync(Slug workspaceId, Slug projectId, ClaimsPrincipal user)
    {
        Project project;
        try
        {
            project = projectManager.GetProjectById(workspaceId, projectId);
        }
        catch (ProjectNotFoundException)
        {
            return false;
        }

        if (project.Status == Status.Active)
        {
            return true;
        }

        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var authResult = await authorizationService.AuthorizeAsync(user, WorkspaceAdminPolicy.Name);
        return authResult.Succeeded;
    }
}
