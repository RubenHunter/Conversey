using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Administration;

namespace Conversey.BL.Administration;

public class ProjectManager: IProjectManager
{
    private readonly IProjectRepository _projectRepository;

    public ProjectManager(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }


    public Project GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug)
    {
        return _projectRepository.ReadProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(slug) ?? throw new ProjectNotFoundException(slug.Text);
    }

    public Youth GetYouthByToken(Guid token)
    {
        return _projectRepository.ReadYouthByToken(token) ?? throw new YouthNotFoundException(token);
    }

    public Youth AddYouth(Guid token, string email, Slug projectSlug)
    {
        var project = _projectRepository.ReadProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(projectSlug);
        if (project == null) throw new ProjectNotFoundException(projectSlug.Text);

        var youth = new Youth
        {
            Id = token,
            Email = email,
            Project = project
        };
        Validate(youth);
        _projectRepository.CreateYouth(youth);
        return youth;
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
