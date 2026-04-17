using Conversey.BL.Domain.Administration;
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
   /// <summary>
   /// Submits an idea to a topic.
   /// </summary>
   /// <param name="workspaceId">The workspace the <see cref="Domain.Administration.Project">Project</see> belongs to.</param>
   /// <param name="projectId">The project the <see cref="Domain.Administration.Topic">Topic</see> belongs to.</param>
   /// <param name="topicId">The topic the <see cref="Idea"/> belongs to.</param>
   /// <param name="youthId">The <see cref="Domain.Administration.Youth">Youth</see> who wrote the idea.</param>
   /// <param name="ideaContent">The content of the idea.</param>
   /// <returns>Whether the idea is <see cref="SubmissionResponse.Pending">Pending</see> or <see cref="SubmissionResponse.Approved">Approved</see>.</returns>
    SubmissionResponse SubmitIdea(Slug workspaceId, Slug projectId, int topicId, Guid youthId, string ideaContent);
    Idea GetIdeaById(Slug workspaceId, Slug projectId, int topicId, int ideaId);
    Idea GetIdea(Topic topic, int ideaId);
    Idea GetIdeaByIdWithProjectAndResponses(Slug workspaceId, Slug projectId, int topicId, int ideaId);
    IEnumerable<Idea> GetIdeasFromProjectByYouthId(Slug workspaceId, Slug projectId, Guid youthId);
    IEnumerable<Idea> GetIdeasByProjectIdAndTopicId(Slug workspaceId, Slug projectId, int topicId);
    Idea ChangeIdea(Slug workspaceId, Slug projectId, int topicId, int ideaId, ModerationStatus newStatus, string newContent);

    
    ResponseSubmissionResponse AddResponse(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, string responseText);
    IdeaResponse GetResponse(Idea ideaId, int responseId);
    IEnumerable<IdeaResponse> GetApprovedResponsesByYouth(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId);
    IdeaResponse ChangeResponse(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, int responseId, ModerationStatus newStatus, string responseText);

    IdeaReaction AddIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, string emoji);
    IEnumerable<IdeaReaction> GetIdeaReactionsByIdeaId(Slug workspaceId, Slug projectId, int topicId, int ideaId);
    void RemoveIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, int reactionId);

    ResponseReaction GetResponseReaction(IdeaResponse response, int reactionId);
    ResponseReaction AddResponseReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, Guid youthId, string emoji);
    IEnumerable<ResponseReaction> GetResponseReactionsByResponseId(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId);
    void RemoveResponseReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, Guid youthId, int reactionId);
}
