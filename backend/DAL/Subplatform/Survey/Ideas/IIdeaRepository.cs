using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.DAL.Subplatform.Survey.Ideas;

public interface IIdeaRepository
{
    void CreateIdea(Idea idea);
    Idea ReadIdeaById(int ideaId);
    Idea ReadIdeaByIdWithProject(int ideaId);
    Idea ReadIdeaByIdWithResponses(int ideaId);
    Idea ReadIdeaByIdWithProjectAndResponses(int ideaId);

    IReadOnlyCollection<Idea> ReadAllIdeas();
    IReadOnlyCollection<Idea> ReadAllIdeasWithProject();
    IReadOnlyCollection<Idea> ReadAllIdeasWithResponses();
    IReadOnlyCollection<Idea> ReadAllIdeasWithProjectAndResponses();

    IReadOnlyCollection<Idea> ReadIdeasFromProjectByProjectId(int projectId);
    IReadOnlyCollection<Idea> ReadIdeasFromProjectByProjectIdWithResponses(int projectId);

    void UpdateIdea(Idea idea);
    bool DeleteIdea(int ideaId);

    void CreateResponse(Response response);
    Response ReadResponseById(int responseId);
    Response ReadResponseByIdWithIdea(int responseId);
    IReadOnlyCollection<Response> ReadResponsesFromIdeaByIdeaId(int ideaId);
    IReadOnlyCollection<Response> ReadResponsesFromIdeaByIdeaIdWithIdea(int ideaId);
    void UpdateResponse(Response response);
    bool DeleteResponse(int responseId);
}
