using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public interface IIdeaManager
{
    void AddIdea(string content, int projectId);
    Idea GetIdeaById(int ideaId);
    IReadOnlyCollection<Idea> GetAllIdeas();
    IReadOnlyCollection<Idea> GetIdeasByProjectId(int projectId);
    Idea EditIdea(Idea idea);
    void RemoveIdea(int ideaId);
    Response AddResponse(string text, int ideaId);
    Response GetResponseById(int responseId);
    IReadOnlyCollection<Response> GetResponsesByIdeaId(int ideaId);
    Response EditResponse(Response response);
    void RemoveResponse(int responseId);
}
