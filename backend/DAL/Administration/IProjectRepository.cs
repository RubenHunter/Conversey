using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IProjectRepository
{
    public Project ReadProjectById(int projectId);
    Project ReadProjectByIdWithTopics(int projectId);
    Project ReadProjectByIdWithQuestions(int projectId);
    Project ReadProjectByIdWithTopicsAndQuestions(int projectId);
    Project ReadProjectByIdWithWorkspaceAndQuestions(int projectId);
    Project ReadProjectByIdWithWorkspaceTopicsYouthsAndQuestions(int projectId);

    Project ReadProjectBySlug(Slug slug);
    Project ReadProjectBySlugWithTopics(Slug slug);
    Project ReadProjectBySlugWithQuestions(Slug slug);
    Project ReadProjectBySlugWithTopicsAndQuestions(Slug slug);
    Project ReadProjectBySlugWithWorkspaceAndQuestions(Slug slug);
    Project ReadProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(Slug slug);

    IReadOnlyCollection<Project> ReadAllProjects();
    IReadOnlyCollection<Project> ReadAllProjectsWithTopics();
    IReadOnlyCollection<Project> ReadAllProjectsWithQuestions();
    IReadOnlyCollection<Project> ReadAllProjectsWithTopicsAndQuestions();

    IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceId(int workspaceId);
    IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithTopics(int workspaceId);
    IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithQuestions(int workspaceId);
    IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(int workspaceId);

    IReadOnlyCollection<Topic> ReadTopicsFromProjectByProjectId(int projectId);
    IReadOnlyCollection<Youth> ReadYouthsFromProjectByProjectId(int projectId);
    void CreateProject(Project project);
    void UpdateProject(Project project);
    bool DeleteProject(int projectId);
    Topic ReadTopicById(int topicId);
    Topic ReadTopicByIdWithProject(int topicId);
    void CreateTopic(Topic topic);
    void UpdateTopic(Topic topic);
    bool DeleteTopic(int topicId);
    Youth ReadYouthByToken(string token);
    Youth ReadYouthByTokenWithProject(string token);
    void CreateYouth(Youth youth);
    void UpdateYouth(Youth youth);
    bool DeleteYouth(string token);
}
