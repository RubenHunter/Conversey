using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public interface IIdeaManager
{
    
    SubmissionResponse SubmitIdea(string content, int projectId);

    Idea GetIdeaById(int ideaId);
    Idea GetIdeaByIdWithProject(int ideaId);
    Idea GetIdeaByIdWithResponses(int ideaId);
    Idea GetIdeaByIdWithProjectAndResponses(int ideaId);

    IReadOnlyCollection<Idea> GetAllIdeas();
    IReadOnlyCollection<Idea> GetAllIdeasWithProject();
    IReadOnlyCollection<Idea> GetAllIdeasWithResponses();
    IReadOnlyCollection<Idea> GetAllIdeasWithProjectAndResponses();

    IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectId(int projectId);
    IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectIdWithResponses(int projectId);

    Idea ChangeIdea(Idea idea);
    void RemoveIdea(int ideaId);

    Response AddResponse(string text, int ideaId);
    Response GetResponseById(int responseId);
    Response GetResponseByIdWithIdea(int responseId);
    IReadOnlyCollection<Response> GetResponsesFromIdeaByIdeaId(int ideaId);
    IReadOnlyCollection<Response> GetResponsesFromIdeaByIdeaIdWithIdea(int ideaId);
    Response ChangeResponse(Response response);
    void RemoveResponse(int responseId);
}
