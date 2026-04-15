using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IProjectManager
{
    Project GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug);

    Youth GetYouthByToken(Guid token);
    Youth AddYouth(Guid token, string email, Slug projectSlug);
}
