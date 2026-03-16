namespace Conversey.BL.Subplatform.Survey.Ideation;

public class IdeaNotFoundException(string ideaIdentifier)
    : Exception($"Idea with id {ideaIdentifier} was not found.");
    
public class ResponseNotFoundException(string responseIdentifier)
    : Exception($"Response with id {responseIdentifier} was not found.");

public class IdeaReactionNotFoundException(int ideaId, string youthToken, string emoji)
    : Exception($"Reaction '{emoji}' by youth '{youthToken}' on idea {ideaId} was not found.");

public class ResponseReactionNotFoundException(int responseId, string youthToken, string emoji)
    : Exception($"Reaction '{emoji}' by youth '{youthToken}' on response {responseId} was not found.");
