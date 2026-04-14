namespace Conversey.BL.Administration;

public class ProjectNotFoundException(string projectIdentifier)
    : Exception($"Project {projectIdentifier} was not found.");
    
public class TopicNotFoundException(string topicIdentifier)
    : Exception($"Topic {topicIdentifier} was not found.");
    
public class YouthNotFoundException(Guid youthId)
    : Exception($"Youth with id {youthId} was not found.");
    
public class WorkspaceNotFoundException(string workspaceIdentifier)
    : Exception($"Workspace {workspaceIdentifier} was not found.");