using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;

namespace Conversey.DAL.Ideation;

public interface IIdeaRepository
{
    void CreateIdea(Idea idea);
    Idea ReadIdeaById(int ideaId);
    Idea ReadIdeaByIdWithProjectAndResponses(int ideaId);

    IReadOnlyCollection<Idea> ReadIdeasFromProjectByYouthToken(Slug projectId, Guid youthToken);

    IReadOnlyCollection<Idea> ReadIdeasFromTopicByProjectSlugAndTopicId(Slug projectSlug, int topicId);

    
    void UpdateIdea(Idea idea);

    void CreateResponse(IdeaResponse ideaResponse);
    IdeaResponse ReadResponseById(int responseId);
    IdeaResponse ReadResponseByIdWithIdea(int responseId);
    IReadOnlyCollection<IdeaResponse> ReadResponsesFromIdeaByIdeaId(int ideaId);
    void UpdateResponse(IdeaResponse ideaResponse);

    void CreateIdeaReaction(IdeaReaction reaction);
    IdeaReaction ReadIdeaReaction(int ideaId, Guid youthToken, string emoji);
    IReadOnlyCollection<IdeaReaction> ReadIdeaReactionsFromIdeaByIdeaId(int ideaId);
    bool DeleteIdeaReaction(int reactionId);

    void CreateResponseReaction(ResponseReaction reaction);
    ResponseReaction ReadResponseReaction(int responseId, Guid youthId, string emoji);
    IReadOnlyCollection<ResponseReaction> ReadResponseReactionsFromResponseByResponseId(int responseId);
    bool DeleteResponseReaction(int responseId, Guid youthId, int reactionId);
}
