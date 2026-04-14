namespace Conversey.BL.Ideation;

public class IdeaNotFoundException(int ideaId)
    : Exception($"Idea with id {ideaId} was not found.");
    
public class ResponseNotFoundException(string responseIdentifier)
    : Exception($"Response with id {responseIdentifier} was not found.");

public class IdeaReactionNotFoundException(int reactionId)
    : Exception($"Reaction {reactionId} was not found.");

public class ResponseReactionNotFoundException(int responseId, string youthToken, string emoji)
    : Exception($"Reaction '{emoji}' by youth '{youthToken}' on response {responseId} was not found.");
