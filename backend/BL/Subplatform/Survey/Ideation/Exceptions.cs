namespace Conversey.BL.Subplatform.Survey.Ideation;

public class IdeaNotFoundException(string ideaIdentifier)
    : Exception($"Idea with id {ideaIdentifier} was not found.");
    
public class ResponseNotFoundException(string responseIdentifier)
    : Exception($"Response with id {responseIdentifier} was not found.");