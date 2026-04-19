using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;

namespace Conversey.BL.Administration;

public class WorkspaceManager: IWorkspaceManager
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public WorkspaceManager(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public IEnumerable<Workspace> GetAllWorkspaces()
    {
        return _workspaceRepository.ReadAllWorkspaces();
    }

    public Workspace CreateWorkspace(string name)
    {
        var workspace = new Workspace
        {
            Id = Slug.FromName(name),
            Name = name
        };
        
        Validate(workspace);
        
        _workspaceRepository.CreateWorkspace(workspace);
        return workspace;
    }

    public Workspace GetWorkspaceById(Slug workspaceId)
    {
        var workspace = _workspaceRepository.ReadWorkspaceById(workspaceId);
        return workspace ?? throw new WorkspaceNotFoundException(workspaceId);
    }

    private void Validate(object obj)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(obj);

        if (!Validator.TryValidateObject(obj, context, validationResults, true))
        {
            throw new ValidationException(string.Join("; ", validationResults.Select(r => r.ErrorMessage)));
        }
    }

}