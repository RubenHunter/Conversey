using Conversey.BL.Domain.Subplatform.Survey;

namespace Conversey.BL.Subplatform.Survey;

public interface IProjectManager
{
    Project GetProjectById(int projectId);
    IReadOnlyCollection<Project> GetAllProjects();
    IReadOnlyCollection<Project> GetProjectsByWorkspaceId(int workspaceId);
    Project AddProject(string title, string description, Status status, DateTime startDate, DateTime endDate, InteractionType interactionForm, int workspaceId);
    Project EditProject(Project project);
    void RemoveProject(int projectId);
    Topic GetTopicById(int topicId);
    IReadOnlyCollection<Topic> GetTopicsByProjectId(int projectId);
    Topic AddTopic(string name, string context, int projectId);
    Topic EditTopic(Topic topic);
    void RemoveTopic(int topicId);
    Youth GetYouthByToken(string token);
    IReadOnlyCollection<Youth> GetYouthsByProjectId(int projectId);
    Youth AddYouth(string token, string email, int projectId);
    Youth EditYouth(Youth youth);
    void RemoveYouth(string token);
}
