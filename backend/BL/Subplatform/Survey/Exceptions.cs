namespace Conversey.BL.Subplatform.Survey;

public class ProjectNotFoundException(string projectIdentifier)
    : Exception($"Project {projectIdentifier} was not found.");
    
public class TopicNotFoundException(string topicIdentifier)
    : Exception($"Topic {topicIdentifier} was not found.");
    
public class YouthNotFoundException(string youthIdentifier)
    : Exception($"Youth with id {youthIdentifier} was not found.");