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

    public Workspace CreateWorkspace(string name, Slug slug)
    {
        var workspace = new Workspace
        {
            Id = slug,
            Name = name
        };
        if (SlugExists(workspace.Id)) throw new ValidationException($"Workspace Slug '{workspace.Id.Text}' already exists.");
        
        Validate(workspace);
        
        _workspaceRepository.CreateWorkspace(workspace);
        return workspace;
    }

    public Workspace GetWorkspaceBySlug(Slug slug)
    {
        var workspace = _workspaceRepository.ReadWorkspaceBySlug(slug);
        return workspace ?? throw new WorkspaceNotFoundException(slug.Text);
    }

    public Workspace GetWorkspaceById(Slug id)
    {
        var workspace = _workspaceRepository.ReadWorkspaceById(id);
        return workspace ?? throw new WorkspaceNotFoundException(id.Text);
    }

    private bool SlugExists(Slug slug)
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