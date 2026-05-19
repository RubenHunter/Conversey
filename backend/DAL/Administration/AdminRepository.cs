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
    private readonly IUserStore<IdentityUser> _userStore;

    public AdminRepository(ConverseyDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUserStore<IdentityUser> userStore)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _userStore = userStore;
    }

    public async Task<WorkspaceAdmin> ReadWorkspaceAdminById(Guid id)
    {
        var workspaceAdmin = await _dbContext.WorkspaceAdmins
            .Include(workspaceAdminUser => workspaceAdminUser.Workspace)
            .FirstOrDefaultAsync(wau => wau.Id == id.ToString());
        return new WorkspaceAdmin
        {
            Id = Guid.Parse(workspaceAdmin?.Id ?? throw new InvalidOperationException()),
            Email = workspaceAdmin.Email,
            Username = workspaceAdmin.UserName ?? workspaceAdmin.Email ?? string.Empty,
            PhoneNumber = workspaceAdmin.PhoneNumber,
            Workspace = workspaceAdmin.Workspace,
            FirstLogin = workspaceAdmin.FirstLogin,
        };
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
                Email = wau.Email,
                Username = wau.UserName ?? wau.Email ?? string.Empty,
                PhoneNumber = wau.PhoneNumber,
                FirstLogin = wau.FirstLogin,
            })
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<WorkspaceAdminUser> ReadAllWorkspaceAdmins()
    {
        return _dbContext.WorkspaceAdmins.ToList().AsReadOnly();
    }

    public async Task CreateWorkspaceAdmin(WorkspaceAdmin workspaceAdmin)
    {
        var workspaceAmin = new WorkspaceAdminUser
        {
            Email = workspaceAdmin.Email,
            UserName = string.IsNullOrWhiteSpace(workspaceAdmin.Username) ? workspaceAdmin.Email : workspaceAdmin.Username,
            EmailConfirmed = true,
            PhoneNumber = workspaceAdmin.PhoneNumber,
            Workspace = workspaceAdmin.Workspace,
            FirstLogin = true
        };
        var result = await _userManager.CreateAsync(workspaceAmin, "Test123!");
        var roleResult = await _userManager.AddToRoleAsync(workspaceAmin, "WorkspaceAdmin");
        if (!result.Succeeded || !roleResult.Succeeded)
        {
            throw new Exception("Creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description) + string.Join(", ", roleResult.Errors.Select(e => e.Description))));
        }
    }

    public async Task UpdateWorkspaceAdmin(WorkspaceAdmin workspaceAdmin)
    {
        var existingUser = _dbContext.WorkspaceAdmins
            .FirstOrDefault(wau => wau.Id == workspaceAdmin.Id.ToString());

        if (existingUser == null)
            throw new KeyNotFoundException($"Workspace Admin with ID {workspaceAdmin.Id} not found.");

        existingUser.Email = workspaceAdmin?.Email;
        existingUser.UserName = string.IsNullOrWhiteSpace(workspaceAdmin?.Username) ? workspaceAdmin?.Email : workspaceAdmin.Username;
        existingUser.PhoneNumber = workspaceAdmin?.PhoneNumber;
        existingUser.Workspace = workspaceAdmin?.Workspace;

        var result = await _userManager.UpdateAsync(existingUser);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to update Workspace Admin: {errors}");
        }
    }

    public async Task DeleteWorkspaceAdmin(Guid workspaceAdminId)
    {
        var workspaceAdmin = await _dbContext.WorkspaceAdmins.FirstOrDefaultAsync(wau => wau.Id == workspaceAdminId.ToString());
        if (workspaceAdmin == null)
            throw new KeyNotFoundException();
        

        var result = await _userManager.DeleteAsync(workspaceAdmin);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to Delete Workspace Admin: {errors}");
        }
    }

    public async Task<(bool EmailExists, bool UsernameExists)> CheckWorkspaceAdminConflicts(
        Slug workspaceId,
        string email,
        string username,
        Guid? excludeWorkspaceAdminId = null)
    {
        var query = _dbContext.WorkspaceAdmins
            .AsNoTracking()
            .Where(wau => wau.Workspace.Id == workspaceId);

        if (excludeWorkspaceAdminId.HasValue)
        {
            var excludedId = excludeWorkspaceAdminId.Value.ToString();
            query = query.Where(wau => wau.Id != excludedId);
        }

        var normalizedEmail = (email ?? string.Empty).Trim().ToUpperInvariant();
        var normalizedUsername = (username ?? string.Empty).Trim().ToUpperInvariant();

        var emailExists = !string.IsNullOrWhiteSpace(normalizedEmail) &&
                          await query.AnyAsync(wau => wau.Email != null && wau.Email.ToUpper() == normalizedEmail);

        var usernameExists = !string.IsNullOrWhiteSpace(normalizedUsername) &&
                             await query.AnyAsync(wau => wau.UserName != null && wau.UserName.ToUpper() == normalizedUsername);

        return (emailExists, usernameExists);
    }
    
}
