using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;

namespace Conversey.DAL.Ideation;

public interface IIdeaRepository
{
    void CreateIdea(Idea idea);
    Idea ReadIdeaById(int ideaId);
    Idea ReadIdeaByIdWithProject(int ideaId);
    Idea ReadIdeaByIdWithResponses(int ideaId);
    Idea ReadIdeaByIdWithProjectAndResponses(int ideaId);
    Idea ReadIdeaByIdWithTopicAndYouthAndReactionsAndProjectWithWorkspace(int ideaId);

    IReadOnlyCollection<Idea> ReadAllIdeas();
    IReadOnlyCollection<Idea> ReadAllIdeasWithProject();
    IReadOnlyCollection<Idea> ReadAllIdeasWithResponses();
    IReadOnlyCollection<Idea> ReadAllIdeasWithProjectAndResponses();

    IReadOnlyCollection<Idea> ReadIdeasFromProjectByProjectId(int projectId);
    IReadOnlyCollection<Idea> ReadIdeasFromProjectByProjectIdWithResponses(int projectId);
    IReadOnlyCollection<Idea> ReadIdeasFromProjectByYouthToken(int projectId, string youthToken);

    IReadOnlyCollection<Idea> ReadIdeasByProjectIdAndTopicId(Slug projectId, int topicId);

    
    void UpdateIdea(Idea idea);
    bool DeleteIdea(int ideaId);

    void CreateResponse(Response response);
    Response ReadResponseById(int responseId);
    Response ReadResponseByIdWithIdea(int responseId);
    IReadOnlyCollection<Response> ReadResponsesFromIdeaByIdeaId(int ideaId);
    IReadOnlyCollection<Response> ReadResponsesFromIdeaByIdeaIdWithIdea(int ideaId);
    void UpdateResponse(Response response);
    bool DeleteResponse(int responseId);

    void CreateIdeaReaction(IdeaReaction reaction);
    IdeaReaction ReadIdeaReaction(int ideaId, string youthToken, string emoji);
    IReadOnlyCollection<IdeaReaction> ReadIdeaReactionsByIdeaId(int ideaId);
    bool DeleteIdeaReaction(int reactionId);

    void CreateResponseReaction(ResponseReaction reaction);
    ResponseReaction ReadResponseReaction(int responseId, string youthToken, string emoji);
    IReadOnlyCollection<ResponseReaction> ReadResponseReactionsFromResponseByResponseId(int responseId);
    bool DeleteResponseReaction(int responseId, string youthToken, string emoji);
}
