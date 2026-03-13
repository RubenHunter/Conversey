using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.DAL.Subplatform.Survey;
using Conversey.DAL.Subplatform.Survey.Ideas;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public class IdeaManager: IIdeaManager
{

    private readonly IIdeaRepository _repository;
    private readonly IProjectRepository _projectRepository;

    public IdeaManager(IIdeaRepository repository, IProjectRepository projectRepository)
    {
        _repository = repository;
        _projectRepository = projectRepository;
    }

    public void AddIdea(string content, int projectId)
    {
        Project forProject = _projectRepository.ReadProjectById(projectId);
        if (forProject == null)
        {
            throw new ArgumentException(nameof(projectId) + " with value " + projectId + " does not exist.");
        }
        
        var idea = new Idea
        {
            Content = content,
            Project = forProject
        };
        _repository.CreateIdea(idea);
    }
}