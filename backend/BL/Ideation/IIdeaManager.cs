using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ideation;

/*
 * DO NOT REMOVE workspaceId, projectId, topicId
 * THEY MAY SEEM UNNECESSARY BUT
 * THESE ARE FOR VALIDATION
 */
public interface IIdeaManager
{
    SubmissionResponse SubmitIdea(string ideaContent, Slug projectId, int topicId, Guid youthId);
    
    Idea GetIdeaById(Slug workspaceId, Slug projectId, int topicId, int ideaId);
    Idea GetIdeaByIdWithProjectAndResponses(int ideaId);
    
    IEnumerable<Idea> GetIdeasFromProjectByYouthToken(int projectId, string youthToken);

    IEnumerable<Idea> GetIdeasByProjectIdAndTopicId(Slug projectId, int topicId);

    
    Idea ChangeIdea(Slug workspaceId, Slug projectId, int topicId, Idea idea);
    
    ResponseSubmissionResponse AddResponse(string text, int ideaId, string youthToken);
    Response GetResponseById(int responseId);
    Response GetResponseByIdWithIdea(int responseId);
    IEnumerable<Response> GetResponsesFromIdeaByIdeaId(int ideaId);
    Response ChangeResponse(Response response);

    IdeaReaction AddIdeaReaction(string emoji, int ideaId, string youthToken);
    IEnumerable<IdeaReaction> GetIdeaReactionsByIdeaId(Slug workspaceId, Slug projectId, int topicId, int ideaId);
    void RemoveIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthToken, int reactionId);

    ResponseReaction AddResponseReaction(string emoji, int responseId, string youthToken);
    IEnumerable<ResponseReaction> GetResponseReactionsFromResponseByResponseId(int responseId);
    void RemoveResponseReaction(int responseId, string youthToken, string emoji);
}
