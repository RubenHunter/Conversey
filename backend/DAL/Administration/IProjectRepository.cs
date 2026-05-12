using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IProjectRepository
{
    Project ReadProjectByIdAndWorkspaceId(Slug projectId, Slug workspaceId);
    Project ReadProjectByIdWithWorkspaceAndTopicsAndYouthAndQuestions(Slug projectId);
    Youth ReadYouthByIdAndProjectId(Guid youthId, Slug projectId);
    Topic ReadTopicByIdAndProjectId(int topicId, Slug projectId);
    void CreateYouth(Youth youth);
    void UpdateYouth(Youth youth);
    
    IReadOnlyCollection<Project> ReadAllProjectsFromWorkspaceId(Slug workspaceId);
    
    void CreateProject(Project project);
    void UpdateProject(Project project);
    void DeleteProject(Slug projectId, Slug workspaceId);
    void DeleteAllProjectsFromWorkspaceId(Slug workspaceId);

}
