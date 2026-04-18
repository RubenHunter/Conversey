using Conversey.BL.Domain.Ideation;

namespace Conversey.DAL.Ideation;

public interface IIdeaRepository
{
    void CreateIdea(Idea idea);
    Idea ReadIdeaById(int ideaId);
    Idea ReadIdeaByIdWithProjectAndResponses(int ideaId);
    IReadOnlyCollection<Idea> ReadIdeasByYouthId(Guid youthId);
    IReadOnlyCollection<Idea> ReadIdeasByTopicId(int topicId);

    
    void UpdateIdea(Idea idea);

    void CreateResponse(IdeaResponse ideaResponse);
    IdeaResponse ReadResponseById(int responseId);
    IdeaResponse ReadResponseByIdWithIdea(int responseId);
    void UpdateResponse(IdeaResponse ideaResponse);

    void CreateIdeaReaction(IdeaReaction reaction);
    IdeaReaction ReadIdeaReactionByIdeaIdAndYouthIdAndEmoji(int ideaId, Guid youthId, string emoji);
    IReadOnlyCollection<IdeaReaction> ReadIdeaReactionsByIdeaId(int ideaId);
    bool DeleteIdeaReaction(int reactionId);

    void CreateResponseReaction(ResponseReaction reaction);
    ResponseReaction ReadResponseReactionByResponseIdAndYouthIdAndEmoji(int responseId, Guid youthId, string emoji);
    ResponseReaction ReadResponseReactionByReactionId(int reactionId);
    IReadOnlyCollection<ResponseReaction> ReadResponseReactionsByResponseId(int responseId);
    bool DeleteResponseReaction(int reactionId);
    IEnumerable<IdeaResponse> ReadApprovedResponsesByYouthIdAndIdeaId(int ideaId, Guid youthId);
}
