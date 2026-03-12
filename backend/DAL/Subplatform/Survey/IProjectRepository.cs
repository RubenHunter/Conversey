using Conversey.BL.Domain.Subplatform.Survey;

namespace Conversey.DAL.Subplatform.Survey;

public interface IProjectRepository
{
    public Project ReadProjectById(int projectId);
    IReadOnlyCollection<Project> ReadAllProjects();
    IReadOnlyCollection<Project> ReadProjectsByWorkspaceId(int workspaceId);
    IReadOnlyCollection<Topic> ReadTopicsByProjectId(int projectId);
    IReadOnlyCollection<Youth> ReadYouthsByProjectId(int projectId);
    void CreateProject(Project project);
    void UpdateProject(Project project);
    void DeleteProject(int projectId);
    Topic ReadTopicById(int topicId);
    void CreateTopic(Topic topic);
    void UpdateTopic(Topic topic);
    void DeleteTopic(int topicId);
    Youth ReadYouthByToken(string token);
    void CreateYouth(Youth youth);
    void UpdateYouth(Youth youth);
    void DeleteYouth(string token);
}

