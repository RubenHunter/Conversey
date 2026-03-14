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
            throw new ProjectNotFoundException(projectId.ToString());
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
        return _repository.ReadIdeaById(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
    }

    public Idea GetIdeaByIdWithProject(int ideaId)
    {
        return _repository.ReadIdeaByIdWithProject(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
    }

    public Idea GetIdeaByIdWithResponses(int ideaId)
    {
        return _repository.ReadIdeaByIdWithResponses(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
    }

    public Idea GetIdeaByIdWithProjectAndResponses(int ideaId)
    {
        return _repository.ReadIdeaByIdWithProjectAndResponses(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
    }

    public IReadOnlyCollection<Idea> GetAllIdeas()
    {
        return _repository.ReadAllIdeas();
    }

    public IReadOnlyCollection<Idea> GetAllIdeasWithProject()
    {
        return _repository.ReadAllIdeasWithProject();
    }

    public IReadOnlyCollection<Idea> GetAllIdeasWithResponses()
    {
        return _repository.ReadAllIdeasWithResponses();
    }

    public IReadOnlyCollection<Idea> GetAllIdeasWithProjectAndResponses()
    {
        return _repository.ReadAllIdeasWithProjectAndResponses();
    }

    public IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectId(int projectId)
    {
        return _repository.ReadIdeasFromProjectByProjectId(projectId);
    }

    public IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectIdWithResponses(int projectId)
    {
        return _repository.ReadIdeasFromProjectByProjectIdWithResponses(projectId);
    }

    public Idea ChangeIdea(Idea idea)
    {
        Validate(idea);
        _repository.UpdateIdea(idea);
        return idea;
    }

    public void RemoveIdea(int ideaId)
    {
        if (!_repository.DeleteIdea(ideaId))
        {
            throw new IdeaNotFoundException(ideaId.ToString());
        }
    }

    public Response AddResponse(string text, int ideaId)
    {
        var idea = _repository.ReadIdeaById(ideaId);
        if (idea == null) throw new IdeaNotFoundException(ideaId.ToString());

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
        return _repository.ReadResponseById(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
    }

    public Response GetResponseByIdWithIdea(int responseId)
    {
        return _repository.ReadResponseByIdWithIdea(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
    }

    public IReadOnlyCollection<Response> GetResponsesFromIdeaByIdeaId(int ideaId)
    {
        return _repository.ReadResponsesFromIdeaByIdeaId(ideaId);
    }

    public IReadOnlyCollection<Response> GetResponsesFromIdeaByIdeaIdWithIdea(int ideaId)
    {
        return _repository.ReadResponsesFromIdeaByIdeaIdWithIdea(ideaId);
    }

    public Response ChangeResponse(Response response)
    {
        Validate(response);
        _repository.UpdateResponse(response);
        return response;
    }

    public void RemoveResponse(int responseId)
    {
        if (!_repository.DeleteResponse(responseId))
        {
            throw new ResponseNotFoundException(responseId.ToString());
        }
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
