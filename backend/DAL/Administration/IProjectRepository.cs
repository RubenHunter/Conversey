using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.DAL.Administration;

public interface IProjectRepository
{
    public Project ReadProjectById(Slug projectSlug);
    Project ReadProjectByIdWithTopics(Slug projectSlug);
    Project ReadProjectByIdWithQuestions(Slug projectSlug);
    Project ReadProjectByIdWithTopicsAndQuestions(Slug projectSlug);
    Project ReadProjectByIdWithWorkspaceAndQuestions(Slug projectSlug);
    Project ReadProjectByIdWithWorkspaceTopicsYouthsAndQuestions(Slug projectSlug);

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

    IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceId(Slug workspaceSlug);
    IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithTopics(Slug workspaceSlug);
    IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithQuestions(Slug workspaceSlug);
    IReadOnlyCollection<Project> ReadProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(Slug workspaceSlug);

    IReadOnlyCollection<Topic> ReadTopicsFromProjectByProjectId(Slug projectSlug);
    IReadOnlyCollection<Youth> ReadYouthsFromProjectByProjectId(Slug projectSlug);
    void CreateProject(Project project);
    void UpdateProject(Project project);
    bool DeleteProject(Slug projectSlug);
    Topic ReadTopicById(int topicId);
    Topic ReadTopicByIdWithProject(int topicId);
    void CreateTopic(Topic topic);
    void UpdateTopic(Topic topic);
    bool DeleteTopic(int topicId);
    Youth ReadYouthByToken(Guid token);
    Youth ReadYouthByTokenWithProject(Guid token);
    void CreateYouth(Youth youth);
    void UpdateYouth(Youth youth);
    bool DeleteYouth(Guid token);
}
