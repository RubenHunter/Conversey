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
    
    IEnumerable<Project> GetAllProjectsFromWorkspaceId(Slug workspaceId);

    Project AddProject(Slug workspaceId, string name, string description, DateTime startDate,
        DateTime endDate, InteractionType interactionForm, string imageUrl = "", int nudgingStrength = 3);

    Project SaveProject(Slug workspaceId, string name, string description, DateTime startDate,
        DateTime endDate, InteractionType interactionForm, string imageUrl, int nudgingStrength, Status status, string? slug);

    void EditProject(Project updatedProject);
    void RemoveProject(Slug projectId, Slug workspaceId);
    
    Task<string> UploadProjectImage(Stream stream, string fileName, string contentType);
}
