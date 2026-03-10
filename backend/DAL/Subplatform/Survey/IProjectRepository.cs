using Conversey.BL.Domain.Subplatform.Survey;

namespace Conversey.DAL.Subplatform.Survey;

public interface IProjectRepository
{
    public Project ReadProjectById(int projectId);
}