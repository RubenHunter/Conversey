using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL;
using Microsoft.AspNetCore.Identity;

namespace Conversey.UI_MVC.Models;

public static class AdminExtensions
{
    /// <summary>
    /// Checks if the current admin user is a Conversey Admin (platform-wide administrator).
    /// </summary>
    /// <param name="adminContext">The AdminContext containing the current admin.</param>
    /// <returns>True if the admin is a ConverseyAdmin; otherwise, false.</returns>
    public static bool IsConverseyAdmin(this AdminContext adminContext)
    {
        return adminContext.CurrentAdmin is BL.Domain.Administration.ConverseyAdmin;
    }

    /// <summary>
    /// Checks if the current admin user is a Conversey Admin (platform-wide administrator).
    /// </summary>
    /// <param name="admin">The Admin domain object to check.</param>
    /// <returns>True if the admin is a ConverseyAdmin; otherwise, false.</returns>
    public static bool IsConverseyAdmin(this BL.Domain.Administration.Admin admin)
    {
        return admin is BL.Domain.Administration.ConverseyAdmin;
    }

    /// <summary>
    /// Checks if the current admin user is a Workspace Admin.
    /// </summary>
    /// <param name="adminContext">The AdminContext containing the current admin.</param>
    /// <returns>True if the admin is a WorkspaceAdmin; otherwise, false.</returns>
    public static bool IsWorkspaceAdmin(this AdminContext adminContext)
    {
        return adminContext.CurrentAdmin is BL.Domain.Administration.WorkspaceAdmin;
    }

    /// <summary>
    /// Checks if the current admin user is a Workspace Admin.
    /// </summary>
    /// <param name="admin">The Admin domain object to check.</param>
    /// <returns>True if the admin is a WorkspaceAdmin; otherwise, false.</returns>
    public static bool IsWorkspaceAdmin(this BL.Domain.Administration.Admin admin)
    {
        return admin is BL.Domain.Administration.WorkspaceAdmin;
    }

    /// <summary>
    /// Gets the Workspace ID for the current admin if they are a WorkspaceAdmin.
    /// </summary>
    /// <param name="adminContext">The AdminContext containing the current admin.</param>
    /// <returns>The Workspace ID if the admin is a WorkspaceAdmin; otherwise, null.</returns>
    public static Slug? GetWorkspaceId(this AdminContext adminContext)
    {
        if (adminContext.CurrentAdmin is BL.Domain.Administration.WorkspaceAdmin workspaceAdmin)
        {
            return workspaceAdmin.Workspace?.Id;
        }
        return null;
    }

    /// <summary>
    /// Gets the Workspace for the current admin if they are a WorkspaceAdmin.
    /// </summary>
    /// <param name="adminContext">The AdminContext containing the current admin.</param>
    /// <returns>The Workspace if the admin is a WorkspaceAdmin; otherwise, null.</returns>
    public static Workspace? GetWorkspace(this AdminContext adminContext)
    {
        if (adminContext.CurrentAdmin is BL.Domain.Administration.WorkspaceAdmin workspaceAdmin)
        {
            return workspaceAdmin.Workspace;
        }
        return null;
    }

    /// <summary>
    /// Gets the Workspace for the current IdentityUser if they are a WorkspaceAdminUser.
    /// </summary>
    /// <param name="user">The IdentityUser to check.</param>
    /// <returns>The Workspace if the user is a WorkspaceAdminUser; otherwise, null.</returns>
    public static Workspace? GetWorkspace(this IdentityUser user)
    {
        if (user is WorkspaceAdminUser workspaceAdminUser)
        {
            return workspaceAdminUser.Workspace;
        }
        return null;
    }

    /// <summary>
    /// Gets the Workspace ID for the current IdentityUser if they are a WorkspaceAdminUser.
    /// </summary>
    /// <param name="user">The IdentityUser to check.</param>
    /// <returns>The Workspace ID if the user is a WorkspaceAdminUser; otherwise, null.</returns>
    public static Slug? GetWorkspaceId(this IdentityUser user)
    {
        if (user is WorkspaceAdminUser workspaceAdminUser)
        {
            return workspaceAdminUser.Workspace?.Id;
        }
        return null;
    }

    /// <summary>
    /// Checks if the current IdentityUser is a ConverseyAdminUser.
    /// </summary>
    /// <param name="user">The IdentityUser to check.</param>
    /// <returns>True if the user is a ConverseyAdminUser; otherwise, false.</returns>
    public static bool IsConverseyAdmin(this IdentityUser user)
    {
        return user is ConverseyAdminUser;
    }

    /// <summary>
    /// Checks if the current IdentityUser is a WorkspaceAdminUser.
    /// </summary>
    /// <param name="user">The IdentityUser to check.</param>
    /// <returns>True if the user is a WorkspaceAdminUser; otherwise, false.</returns>
    public static bool IsWorkspaceAdmin(this IdentityUser user)
    {
        return user is WorkspaceAdminUser;
    }

    /// <summary>
    /// Gets the dashboard URL for the current admin.
    /// Returns the appropriate dashboard path based on admin role.
    /// </summary>
    /// <param name="adminContext">The AdminContext containing the current admin.</param>
    /// <returns>The URL to the admin's dashboard.</returns>
    public static string GetDashboardUrl(this AdminContext adminContext)
    {
        if (adminContext.CurrentAdmin is ConverseyAdmin)
            return "/admin/dashboard/conversey";
        if (adminContext.CurrentAdmin is WorkspaceAdmin)
            return "/admin/dashboard/workspace";
        return "/admin";
    }

    /// <summary>
    /// Gets the dashboard URL for the given admin.
    /// </summary>
    /// <param name="admin">The Admin domain object.</param>
    /// <returns>The URL to the admin's dashboard.</returns>
    public static string GetDashboardUrl(this BL.Domain.Administration.Admin admin)
    {
        if (admin is ConverseyAdmin)
            return "/admin/dashboard/conversey";
        if (admin is WorkspaceAdmin)
            return "/admin/dashboard/workspace";
        return "/admin";
    }
}
