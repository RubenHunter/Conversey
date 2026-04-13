using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public interface IIdeaManager
{
    SubmissionResponse SubmitIdea(string content, Slug projectSlug, int topicId, Guid youthToken);

    Idea GetIdeaById(int ideaId);
    Idea GetIdeaByIdWithProject(int ideaId);
    Idea GetIdeaByIdWithResponses(int ideaId);
    Idea GetIdeaByIdWithProjectAndResponses(int ideaId);

    IReadOnlyCollection<Idea> GetAllIdeas();
    IReadOnlyCollection<Idea> GetAllIdeasWithProject();
    IReadOnlyCollection<Idea> GetAllIdeasWithResponses();
    IReadOnlyCollection<Idea> GetAllIdeasWithProjectAndResponses();

    IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectId(Slug projectSlug);
    IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectIdWithResponses(Slug projectSlug);
    IReadOnlyCollection<Idea> GetIdeasFromProjectByYouthToken(Slug projectSlug, Guid youthToken);

    IReadOnlyCollection<Idea> GetIdeasFromTopicByProjectSlugAndTopicId(Slug projectSlug, int topicId);

    
    Idea ChangeIdea(Idea idea);
    void RemoveIdea(int ideaId);
    
    ResponseSubmissionResponse AddResponse(string text, int ideaId, Guid youthToken);
    Response GetResponseById(int responseId);
    Response GetResponseByIdWithIdea(int responseId);
    IReadOnlyCollection<Response> GetResponsesFromIdeaByIdeaId(int ideaId);
    IReadOnlyCollection<Response> GetResponsesFromIdeaByIdeaIdWithIdea(int ideaId);
    Response ChangeResponse(Response response);
    void RemoveResponse(int responseId);

    IdeaReaction AddIdeaReaction(string emoji, int ideaId, Guid youthToken);
    IReadOnlyCollection<IdeaReaction> GetIdeaReactionsFromIdeaByIdeaId(int ideaId);
    void RemoveIdeaReaction(int ideaId, Guid youthToken, string emoji);

    ResponseReaction AddResponseReaction(string emoji, int responseId, Guid youthToken);
    IReadOnlyCollection<ResponseReaction> GetResponseReactionsFromResponseByResponseId(int responseId);
    void RemoveResponseReaction(int responseId, Guid youthToken, string emoji);
}
