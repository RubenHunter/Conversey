using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IProjectManager
{
    Project GetProject(Workspace workspace, Slug projectId);
    Project GetProjectById(Slug workspaceId, Slug projectId);
    Topic GetTopic(Project project, int topicId);
    Youth GetYouth(Project project, Guid youthId);

    Youth AddYouth(Guid token, string email, Slug projectId);
    
    IReadOnlyCollection<Project> GetAllProjectsFromWorkspaceId(Slug workspaceId);

    Project AddProject(Slug workspaceId, string name, string description, Status status, DateTime startDate,
        DateTime endDate, InteractionType interactionForm, int nudgingStrength = 3);

    void EditProject(Project updatedProject);
    void RemoveProject(Slug projectId, Slug workspaceId);
}
