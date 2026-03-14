namespace Conversey.BL.Subplatform.Survey.Ideation;

public class IdeaNotFoundException(string ideaIdentifier)
    : Exception($"Idea with id {ideaIdentifier} was not found.");