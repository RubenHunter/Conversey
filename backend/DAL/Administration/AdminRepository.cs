using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Administration;

public class AdminRepository : IAdminRepository
{
    private readonly ConverseyDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminRepository(ConverseyDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IReadOnlyCollection<WorkspaceAdmin> ReadAllWorkspaceAdminsByWorkspaceIdWithWorkspace(Slug id)
    {
        return _dbContext.WorkspaceAdmins
            .Include(wau => wau.Workspace)
            .Where(wau => wau.Workspace.Id == id)
            .Select(wau => new WorkspaceAdmin
            {
                Id = Guid.Parse(wau.Id),
                Workspace = wau.Workspace,
                Email = wau.Email
            })
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<WorkspaceAdminUser> ReadAllWorkspaceAdmins()
    {
        return _dbContext.WorkspaceAdmins.ToList().AsReadOnly();
    }

    public void CreateWorkspaceAdmin(WorkspaceAdmin workspaceAdmin)
    {
        var workspaceAmin = new WorkspaceAdminUser
        {
            Email = workspaceAdmin.Email,
            UserName = workspaceAdmin.Email,
            EmailConfirmed = true,
            Workspace = workspaceAdmin.Workspace
        };

        _userManager.CreateAsync(workspaceAmin, "Test123!").Wait();


    }
}