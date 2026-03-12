using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.DAL.Subplatform.Survey.Ideas;

public interface IIdeaRepository
{
    void CreateIdea(Idea idea);
    Idea ReadIdeaById(int ideaId);
    IReadOnlyCollection<Idea> ReadAllIdeas();
    IReadOnlyCollection<Idea> ReadIdeasByProjectId(int projectId);
    void UpdateIdea(Idea idea);
    void DeleteIdea(int ideaId);
    void CreateResponse(Response response);
    Response ReadResponseById(int responseId);
    IReadOnlyCollection<Response> ReadResponsesByIdeaId(int ideaId);
    void UpdateResponse(Response response);
    void DeleteResponse(int responseId);
}
