using System.ComponentModel.DataAnnotations;
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
            throw new Exception($"Project with id {projectId} not found");
        }

        var idea = new Idea
        {
            Content = content,
            Project = forProject
        };
        _repository.CreateIdea(idea);
    }

    public Idea GetIdeaById(int ideaId)
    {
        return _repository.ReadIdeaById(ideaId);
    }

    public IReadOnlyCollection<Idea> GetAllIdeas()
    {
        return _repository.ReadAllIdeas();
    }

    public IReadOnlyCollection<Idea> GetIdeasByProjectId(int projectId)
    {
        return _repository.ReadIdeasByProjectId(projectId);
    }

    public Idea EditIdea(Idea idea)
    {
        Validate(idea);
        _repository.UpdateIdea(idea);
        return idea;
    }

    public void RemoveIdea(int ideaId)
    {
        _repository.DeleteIdea(ideaId);
    }

    public Response AddResponse(string text, int ideaId)
    {
        var idea = _repository.ReadIdeaById(ideaId);
        var response = new Response
        {
            text = text,
            Idea = idea,
            createdAt = DateTime.UtcNow
        };
        Validate(response);
        _repository.CreateResponse(response);
        return response;
    }

    public Response GetResponseById(int responseId)
    {
        return _repository.ReadResponseById(responseId);
    }

    public IReadOnlyCollection<Response> GetResponsesByIdeaId(int ideaId)
    {
        return _repository.ReadResponsesByIdeaId(ideaId);
    }

    public Response EditResponse(Response response)
    {
        Validate(response);
        _repository.UpdateResponse(response);
        return response;
    }

    public void RemoveResponse(int responseId)
    {
        _repository.DeleteResponse(responseId);
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

