namespace Conversey.BL.Subplatform.Survey;

public class TopicNotFoundException(string topicIdentifier)
    : Exception($"Topic {topicIdentifier} was not found.");