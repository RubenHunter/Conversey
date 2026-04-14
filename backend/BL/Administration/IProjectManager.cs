using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Administration;

public interface IProjectManager
{
    Project GetProjectById(int projectId);
    Project GetProjectByIdWithTopics(int projectId);
    Project GetProjectByIdWithQuestions(int projectId);
    Project GetProjectByIdWithTopicsAndQuestions(int projectId);
    Project GetProjectByIdWithWorkspaceAndQuestions(int projectId);
    Project GetProjectByIdWithWorkspaceTopicsYouthsAndQuestions(int projectId);

    Project GetProjectBySlug(Slug slug);
    Project GetProjectBySlugWithTopics(Slug slug);
    Project GetProjectBySlugWithQuestions(Slug slug);
    Project GetProjectBySlugWithTopicsAndQuestions(Slug slug);
    Project GetProjectBySlugWithWorkspaceAndQuestions(Slug slug);
    Project GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug);

    IReadOnlyCollection<Project> GetAllProjects();
    IReadOnlyCollection<Project> GetAllProjectsWithTopics();
    IReadOnlyCollection<Project> GetAllProjectsWithQuestions();
    IReadOnlyCollection<Project> GetAllProjectsWithTopicsAndQuestions();

    IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceId(int workspaceId);
    IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithTopics(int workspaceId);
    IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithQuestions(int workspaceId);
    IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(int workspaceId);

    public Project AddProject(string title, string slug, string description, Status status, DateTime startDate,
        DateTime endDate, InteractionType interactionForm, int workspaceId);
    Project ChangeProject(Project project);
    void RemoveProject(int projectId);
    Topic GetTopicById(int topicId);
    IReadOnlyCollection<Topic> GetTopicsFromProjectByProjectId(int projectId);
    Topic AddTopic(string name, string context, int projectId);
    Topic ChangeTopic(Topic topic);
    void RemoveTopic(int topicId);
    Youth GetYouthByToken(string token);
    IReadOnlyCollection<Youth> GetYouthsFromProjectByProjectId(int projectId);
    Youth AddYouth(string token, string email, int projectId);
    Youth ChangeYouth(Youth youth);
    void RemoveYouth(string token);
}
