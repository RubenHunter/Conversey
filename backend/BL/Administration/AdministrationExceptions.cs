using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public class ProjectNotFoundException(Slug projectId)
    : NotFoundException($"Project {projectId}");
    
public class TopicNotFoundException(int topicId)
    : NotFoundException($"Topic {topicId}");
    
public class YouthNotFoundException(Guid youthId)
    : NotFoundException($"Youth {youthId}");
    
public class WorkspaceNotFoundException(Slug workspaceId)
    : NotFoundException($"Workspace {workspaceId}");
    
public class WorkspaceAdminNotFoundException(Guid workspaceAdminId)
    : NotFoundException($"Workspace {workspaceAdminId}");
    
public class ConverseyAdminNotFoundException(Guid converseyAdminId)
    : NotFoundException($"Workspace {converseyAdminId}");