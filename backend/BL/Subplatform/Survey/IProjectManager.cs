using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Subplatform.Survey;

public interface IProjectManager
{
    Project GetProjectById(Slug projectSlug);
    Project GetProjectByIdWithTopics(Slug projectSlug);
    Project GetProjectByIdWithQuestions(Slug projectSlug);
    Project GetProjectByIdWithTopicsAndQuestions(Slug projectSlug);
    Project GetProjectByIdWithWorkspaceAndQuestions(Slug projectSlug);
    Project GetProjectByIdWithWorkspaceTopicsYouthsAndQuestions(Slug projectSlug);

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

    IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceId(Slug workspaceSlug);
    IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithTopics(Slug workspaceSlug);
    IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithQuestions(Slug workspaceSlug);
    IReadOnlyCollection<Project> GetProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(Slug workspaceSlug);

    public Project AddProject(string title, string slug, string description, Status status, DateTime startDate,
        DateTime endDate, InteractionType interactionForm, Slug workspaceSlug);
    Project ChangeProject(Project project);
    void RemoveProject(Slug projectSlug);
    Topic GetTopicById(int topicId);
    IReadOnlyCollection<Topic> GetTopicsFromProjectByProjectId(Slug projectSlug);
    Topic AddTopic(string name, string context, Slug projectSlug);
    Topic ChangeTopic(Topic topic);
    void RemoveTopic(int topicId);
    Youth GetYouthByToken(Guid token);
    IReadOnlyCollection<Youth> GetYouthsFromProjectByProjectId(Slug projectSlug);
    Youth AddYouth(Guid token, string email, Slug projectSlug);
    Youth ChangeYouth(Youth youth);
    void RemoveYouth(Guid token);
}
