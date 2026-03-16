using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public interface IIdeaManager
{
    SubmissionResponse SubmitIdea(string content, int projectId, int topicId, string youthToken);

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
    IReadOnlyCollection<Idea> GetIdeasFromProjectByYouthToken(int projectId, string youthToken);

    IReadOnlyCollection<Idea> GetIdeasFromTopicByProjectSlugAndTopicId(Slug projectSlug, int topicId);

    
    Idea ChangeIdea(Idea idea);
    void RemoveIdea(int ideaId);
    
    ResponseSubmissionResponse AddResponse(string text, int ideaId, string youthToken);
    Response GetResponseById(int responseId);
    Response GetResponseByIdWithIdea(int responseId);
    IReadOnlyCollection<Response> GetResponsesFromIdeaByIdeaId(int ideaId);
    IReadOnlyCollection<Response> GetResponsesFromIdeaByIdeaIdWithIdea(int ideaId);
    Response ChangeResponse(Response response);
    void RemoveResponse(int responseId);

    IdeaReaction AddIdeaReaction(string emoji, int ideaId, string youthToken);
    IReadOnlyCollection<IdeaReaction> GetIdeaReactionsFromIdeaByIdeaId(int ideaId);
    void RemoveIdeaReaction(int ideaId, string youthToken, string emoji);

    ResponseReaction AddResponseReaction(string emoji, int responseId, string youthToken);
    IReadOnlyCollection<ResponseReaction> GetResponseReactionsFromResponseByResponseId(int responseId);
    void RemoveResponseReaction(int responseId, string youthToken, string emoji);
}
