using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;

namespace Conversey.BL.Administration;

public class AdminManager : IAdminManager
{
    private readonly IAdminRepository _adminRepository;
    private readonly IWorkspaceManager _workspaceManager;

    public AdminManager(IAdminRepository adminRepository, IWorkspaceManager workspaceManager)
    {
        _adminRepository = adminRepository;
        _workspaceManager = workspaceManager;
    }

    public async Task<WorkspaceAdmin> GetWorkspaceAdminById(Guid id)
    {
        return await _adminRepository.ReadWorkspaceAdminById(id);
    }

    public IEnumerable<WorkspaceAdmin> GetAllWorkspaceAdminsByWorkspaceIdWithWorkspace(Slug id)
    {
        return _adminRepository.ReadAllWorkspaceAdminsByWorkspaceIdWithWorkspace(id);
    }

    public async Task<WorkspaceAdmin> AddWorkspaceAdmin(string email, string username, string phoneNumber, Slug workspaceId)
    {
        var workspace = _workspaceManager.GetWorkspaceById(workspaceId);

        var workspaceAdmin = new WorkspaceAdmin
        {
            Email = email?.Trim(),
            Username = username?.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
            Workspace = workspace,
            FirstLogin = true
        };
        await EnsureWorkspaceAdminUnique(workspace.Id, workspaceAdmin.Email, workspaceAdmin.Username);
        Validate(workspaceAdmin);
        await _adminRepository.CreateWorkspaceAdmin(workspaceAdmin);
        return workspaceAdmin;
    }

    public async Task EditWorkspaceAdmin(WorkspaceAdmin workspaceAdmin)
    {
        try
        {
            workspaceAdmin.Email = workspaceAdmin.Email?.Trim();
            workspaceAdmin.Username = workspaceAdmin.Username?.Trim();
            workspaceAdmin.PhoneNumber = string.IsNullOrWhiteSpace(workspaceAdmin.PhoneNumber) ? null : workspaceAdmin.PhoneNumber;
            workspaceAdmin.Workspace = _workspaceManager.GetWorkspaceById(workspaceAdmin.Workspace.Id);
            await EnsureWorkspaceAdminUnique(workspaceAdmin.Workspace.Id, workspaceAdmin.Email, workspaceAdmin.Username, workspaceAdmin.Id);
            Validate(workspaceAdmin);
            await _adminRepository.UpdateWorkspaceAdmin(workspaceAdmin);
        }
        catch (KeyNotFoundException e)
        {
            throw new WorkspaceAdminNotFoundException(workspaceAdmin.Id);
        }
    }

    public async Task RemoveWorkspaceAdmin(Guid workspaceAdminId)
    {
        try
        {
            await _adminRepository.DeleteWorkspaceAdmin(workspaceAdminId);
        }
        catch (KeyNotFoundException e)
        {
            throw new WorkspaceAdminNotFoundException(workspaceAdminId);
        }
    }
    
    


    private void Validate(object obj)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(obj);

        if (!Validator.TryValidateObject(obj, context, validationResults, true))
        {
            var ex = new ValidationException("Validation failed");

            // attach structured data
            ex.Data["ValidationResults"] = validationResults;

            throw ex;
        }
    }

    private async Task EnsureWorkspaceAdminUnique(Slug workspaceId, string email, string username, Guid? excludeWorkspaceAdminId = null)
    {
        var (emailExists, usernameExists) = await _adminRepository.CheckWorkspaceAdminConflicts(
            workspaceId,
            email,
            username,
            excludeWorkspaceAdminId);

        if (!emailExists && !usernameExists)
        {
            return;
        }

        var validationResults = new List<ValidationResult>();
        if (emailExists)
        {
            validationResults.Add(new ValidationResult(
                "Email already exists in this workspace.",
                [nameof(Admin.Email)]));
        }

        if (usernameExists)
        {
            validationResults.Add(new ValidationResult(
                "Username already exists in this workspace.",
                [nameof(Admin.Username)]));
        }

        var ex = new ValidationException("Validation failed");
        ex.Data["ValidationResults"] = validationResults;
        throw ex;
    }
}
