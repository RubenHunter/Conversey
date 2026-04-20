using Conversey.BL.Domain.Common;

namespace Conversey.BL.Ideation;

public class IdeaNotFoundException(int ideaId) : NotFoundException($"Idea {ideaId}");

public class ResponseNotFoundException(int responseId) : NotFoundException($"Response {responseId}");

public class IdeaReactionNotFoundException(int reactionId) : NotFoundException($"Idea reaction {reactionId}");

public class ResponseReactionNotFoundException(int reactionId) : NotFoundException($"Response reaction {reactionId}");
