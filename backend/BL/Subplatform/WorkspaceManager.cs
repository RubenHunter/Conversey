using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform;
using Conversey.DAL.Subplatform;

namespace Conversey.BL.Subplatform;

public class WorkspaceManager: IWorkspaceManager
{
    private IWorkspaceRepository _workspaceRepository;

    public WorkspaceManager(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public IReadOnlyCollection<Workspace> GetAllWorkspaces()
    {
        return _workspaceRepository.ReadAllWorkspaces();
    }

    public Workspace CreateWorkspace(string name, string slug)
    {
        var workspace = new Workspace(name, slug, SlugExists);
        Validate(workspace);
        _workspaceRepository.CreateWorkspace(workspace);
        return workspace;
    }

    public Workspace GetWorkspaceBySlug(string slug)
    {
        var workspace = _workspaceRepository.ReadWorkspaceBySlug(slug);
        return workspace ?? throw new KeyNotFoundException("Workspace not found");
    }
    
    public Workspace GetWorkspaceById(int id)
    {
        var workspace = _workspaceRepository.ReadWorkspaceById(id);
        return workspace ?? throw new KeyNotFoundException("Workspace not found");
    }
     
    
    
    
    
    
    private bool SlugExists(string slug)
    {
        return _workspaceRepository.ReadWorkspaceBySlug(slug) != null;
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