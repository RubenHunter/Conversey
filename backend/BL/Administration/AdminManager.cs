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

    public IEnumerable<WorkspaceAdmin> GetAllWorkspaceAdminsByWorkspaceIdWithWorkspace(Slug id)
    {
        return _adminRepository.ReadAllWorkspaceAdminsByWorkspaceIdWithWorkspace(id);
    }

    public WorkspaceAdmin AddWorkspaceAdmin(string email, Slug workspaceId)
    {
        var workspace = _workspaceManager.GetWorkspaceById(workspaceId);

        var workspaceAdmin = new WorkspaceAdmin
        {
            Email = email,
            Workspace = workspace
        };
        Validate(workspaceAdmin);
        _adminRepository.CreateWorkspaceAdmin(workspaceAdmin);
        return workspaceAdmin;
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
}