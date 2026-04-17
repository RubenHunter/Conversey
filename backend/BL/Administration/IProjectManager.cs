using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IProjectManager
{
    Project GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug);
    Project GetProject(Workspace workspace, Slug projectId);
    Project GetProjectById(Slug workspaceId, Slug projectId);
    Topic GetTopic(Project project, int topicId);
    Youth GetYouth(Project project, Guid youthId);

    Youth GetYouthByToken(Guid token);
    Youth AddYouth(Guid token, string email, Slug projectSlug);
}
