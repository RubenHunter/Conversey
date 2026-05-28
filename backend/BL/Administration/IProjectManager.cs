using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IProjectManager
{
    Project GetProjectById(Slug workspaceId, Slug projectId);
    Topic GetTopic(Project project, int topicId);
    Youth GetYouth(Project project, Guid youthId);

    Youth AddYouth(Guid token, string email, Slug projectId);
    
    IEnumerable<Project> GetAllProjectsFromWorkspaceId(Slug workspaceId);

    Project SaveProject(Slug workspaceId, string name, string description, DateTime startDate,
        DateTime endDate, InteractionType interactionForm, string imageUrl, int nudgingStrength, int? minAge, int? maxAge,
        Status status, string slug, ProjectTheme theme = null);

    void EditProject(Project updatedProject);
    void RemoveProject(Slug projectId, Slug workspaceId);
    
    Task<string> UploadProjectImage(Stream stream, string fileName, string contentType);

    void AddTopic(Slug projectId, Slug workspaceId, string name, string context);
}
