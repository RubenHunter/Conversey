using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IProjectRepository
{
    Project ReadProjectByIdWithTopics(Slug projectSlug);
    Project ReadProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug);
    Youth ReadYouthByToken(Guid token);
    Youth ReadYouthByTokenWithProject(Guid token);
    void CreateYouth(Youth youth);
}
