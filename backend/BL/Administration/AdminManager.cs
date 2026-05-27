using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
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

    public IEnumerable<WorkspaceAdmin> GetAllWorkspaceAdmins()
    {
        return _adminRepository.ReadAllWorkspaceAdmins();
    }

    public async Task<(WorkspaceAdmin Admin, string OneTimePassword)> AddWorkspaceAdmin(string email, string username, string phoneNumber, Slug workspaceId)
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
        var oneTimePassword = GenerateOneTimePassword();
        await _adminRepository.CreateWorkspaceAdmin(workspaceAdmin, oneTimePassword);
        return (workspaceAdmin, oneTimePassword);
    }

    public Task SetWorkspaceAdminFirstLogin(Guid workspaceAdminId, bool isFirstLogin)
    {
        return _adminRepository.SetWorkspaceAdminFirstLogin(workspaceAdminId, isFirstLogin);
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

    public IEnumerable<ConverseyAdmin> GetAllConverseyAdmins()
    {
        return _adminRepository.ReadAllConverseyAdmins();
    }

    public async Task<(ConverseyAdmin Admin, string OneTimePassword)> AddConverseyAdmin(string email, string username, string phoneNumber)
    {
        var converseyAdmin = new ConverseyAdmin
        {
            Email = email?.Trim(),
            Username = username?.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
            FirstLogin = true
        };
        await EnsureConverseyAdminUnique(converseyAdmin.Email, converseyAdmin.Username);
        Validate(converseyAdmin);
        var oneTimePassword = GenerateOneTimePassword();
        await _adminRepository.CreateConverseyAdmin(converseyAdmin, oneTimePassword);
        return (converseyAdmin, oneTimePassword);
    }

    public Task SetConverseyAdminFirstLogin(Guid converseyAdminId, bool isFirstLogin)
    {
        return _adminRepository.SetConverseyAdminFirstLogin(converseyAdminId, isFirstLogin);
    }

    public async Task EditConverseyAdmin(ConverseyAdmin converseyAdmin)
    {
        try
        {
            converseyAdmin.Email = converseyAdmin.Email?.Trim();
            converseyAdmin.Username = converseyAdmin.Username?.Trim();
            converseyAdmin.PhoneNumber = string.IsNullOrWhiteSpace(converseyAdmin.PhoneNumber) ? null : converseyAdmin.PhoneNumber;
            
            await EnsureConverseyAdminUnique(converseyAdmin.Email, converseyAdmin.Username, converseyAdmin.Id);
            Validate(converseyAdmin);
            await _adminRepository.UpdateConverseyAdmin(converseyAdmin);
        }
        catch (KeyNotFoundException)
        {
            throw new ConverseyAdminNotFoundException(converseyAdmin.Id);
        }
    }

    public async Task RemoveConverseyAdmin(Guid converseyAdminId)
    {
        try
        {
            await _adminRepository.DeleteConverseyAdmin(converseyAdminId);
        }
        catch (KeyNotFoundException)
        {
            throw new ConverseyAdminNotFoundException(converseyAdminId);
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

    private async Task EnsureConverseyAdminUnique(string email, string username, Guid? excludeConverseyAdminId = null)
    {
        var (emailExists, usernameExists) = await _adminRepository.CheckConverseyAdminConflicts(
            email,
            username,
            excludeConverseyAdminId);

        if (!emailExists && !usernameExists)
        {
            return;
        }

        var validationResults = new List<ValidationResult>();
        if (emailExists)
        {
            validationResults.Add(new ValidationResult(
                "Email already exists.",
                [nameof(Admin.Email)]));
        }

        if (usernameExists)
        {
            validationResults.Add(new ValidationResult(
                "Username already exists.",
                [nameof(Admin.Username)]));
        }

        var ex = new ValidationException("Validation failed");
        ex.Data["ValidationResults"] = validationResults;
        throw ex;
    }

    private static string GenerateOneTimePassword()
    {
        const int length = 12;
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*-_+";
        var all = string.Concat(upper, lower, digits, symbols);

        var chars = new List<char>
        {
            upper[RandomNumberGenerator.GetInt32(upper.Length)],
            lower[RandomNumberGenerator.GetInt32(lower.Length)],
            digits[RandomNumberGenerator.GetInt32(digits.Length)],
            symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
        };

        for (var i = chars.Count; i < length; i++)
        {
            chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        for (var i = chars.Count - 1; i > 0; i--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[swapIndex]) = (chars[swapIndex], chars[i]);
        }

        return new string(chars.ToArray());
    }
}
