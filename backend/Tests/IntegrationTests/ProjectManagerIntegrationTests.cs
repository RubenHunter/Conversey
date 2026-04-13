using Conversey.BL.Subplatform.Survey;
using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.DAL;
using Microsoft.Extensions.DependencyInjection;
using Tests.IntegrationTests.Infrastructure;

namespace Tests.IntegrationTests;

public class ProjectManagerIntegrationTests : IClassFixture<ManagerIntegrationTestFixture>
{
    private readonly ManagerIntegrationTestFixture _fixture;

    public ProjectManagerIntegrationTests(ManagerIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    public static IEnumerable<object[]> ProjectByIdReaders()
    {
        yield return new object[] { "GetProjectById", (Func<IProjectManager, int, Project>)((manager, id) => manager.GetProjectById(id)) };
        yield return new object[] { "GetProjectByIdWithTopics", (Func<IProjectManager, int, Project>)((manager, id) => manager.GetProjectByIdWithTopics(id)) };
        yield return new object[] { "GetProjectByIdWithQuestions", (Func<IProjectManager, int, Project>)((manager, id) => manager.GetProjectByIdWithQuestions(id)) };
        yield return new object[] { "GetProjectByIdWithTopicsAndQuestions", (Func<IProjectManager, int, Project>)((manager, id) => manager.GetProjectByIdWithTopicsAndQuestions(id)) };
        yield return new object[] { "GetProjectByIdWithWorkspaceAndQuestions", (Func<IProjectManager, int, Project>)((manager, id) => manager.GetProjectByIdWithWorkspaceAndQuestions(id)) };
        yield return new object[] { "GetProjectByIdWithWorkspaceTopicsYouthsAndQuestions", (Func<IProjectManager, int, Project>)((manager, id) => manager.GetProjectByIdWithWorkspaceTopicsYouthsAndQuestions(id)) };
    }

    public static IEnumerable<object[]> ProjectBySlugReaders()
    {
        yield return new object[] { "GetProjectBySlug", (Func<IProjectManager, Slug, Project>)((manager, slug) => manager.GetProjectBySlug(slug)) };
        yield return new object[] { "GetProjectBySlugWithTopics", (Func<IProjectManager, Slug, Project>)((manager, slug) => manager.GetProjectBySlugWithTopics(slug)) };
        yield return new object[] { "GetProjectBySlugWithQuestions", (Func<IProjectManager, Slug, Project>)((manager, slug) => manager.GetProjectBySlugWithQuestions(slug)) };
        yield return new object[] { "GetProjectBySlugWithTopicsAndQuestions", (Func<IProjectManager, Slug, Project>)((manager, slug) => manager.GetProjectBySlugWithTopicsAndQuestions(slug)) };
        yield return new object[] { "GetProjectBySlugWithWorkspaceAndQuestions", (Func<IProjectManager, Slug, Project>)((manager, slug) => manager.GetProjectBySlugWithWorkspaceAndQuestions(slug)) };
        yield return new object[] { "GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions", (Func<IProjectManager, Slug, Project>)((manager, slug) => manager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(slug)) };
    }

    public static IEnumerable<object[]> AllProjectReaders()
    {
        yield return new object[] { "GetAllProjects", (Func<IProjectManager, IReadOnlyCollection<Project>>)(manager => manager.GetAllProjects()) };
        yield return new object[] { "GetAllProjectsWithTopics", (Func<IProjectManager, IReadOnlyCollection<Project>>)(manager => manager.GetAllProjectsWithTopics()) };
        yield return new object[] { "GetAllProjectsWithQuestions", (Func<IProjectManager, IReadOnlyCollection<Project>>)(manager => manager.GetAllProjectsWithQuestions()) };
        yield return new object[] { "GetAllProjectsWithTopicsAndQuestions", (Func<IProjectManager, IReadOnlyCollection<Project>>)(manager => manager.GetAllProjectsWithTopicsAndQuestions()) };
    }

    public static IEnumerable<object[]> WorkspaceProjectReaders()
    {
        yield return new object[] { "GetProjectsFromWorkspaceByWorkspaceId", (Func<IProjectManager, int, IReadOnlyCollection<Project>>)((manager, id) => manager.GetProjectsFromWorkspaceByWorkspaceId(id)) };
        yield return new object[] { "GetProjectsFromWorkspaceByWorkspaceIdWithTopics", (Func<IProjectManager, int, IReadOnlyCollection<Project>>)((manager, id) => manager.GetProjectsFromWorkspaceByWorkspaceIdWithTopics(id)) };
        yield return new object[] { "GetProjectsFromWorkspaceByWorkspaceIdWithQuestions", (Func<IProjectManager, int, IReadOnlyCollection<Project>>)((manager, id) => manager.GetProjectsFromWorkspaceByWorkspaceIdWithQuestions(id)) };
        yield return new object[] { "GetProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions", (Func<IProjectManager, int, IReadOnlyCollection<Project>>)((manager, id) => manager.GetProjectsFromWorkspaceByWorkspaceIdWithTopicsAndQuestions(id)) };
    }

    #region GetProject

    [Theory]
    [MemberData(nameof(ProjectByIdReaders))]
    public void ProjectByIdReaders_WhenProjectExists_ShouldReturnProject(string _, Func<IProjectManager, int, Project> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var project = readMethod(projectManager, projectId);

        // Assert
        Assert.Equal(ManagerSeedData.ProjectTitle, project.Title);
        Assert.Equal(ManagerSeedData.ProjectSlug.Text, project.Slug.Text);
    }

    [Theory]
    [MemberData(nameof(ProjectByIdReaders))]
    public void ProjectByIdReaders_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException(string _, Func<IProjectManager, int, Project> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var act = () => readMethod(projectManager, -9999);

        // Assert
        Assert.Throws<ProjectNotFoundException>(act);
    }

    [Theory]
    [MemberData(nameof(ProjectBySlugReaders))]
    public void ProjectBySlugReaders_WhenProjectExists_ShouldReturnProject(string _, Func<IProjectManager, Slug, Project> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var project = readMethod(projectManager, ManagerSeedData.ProjectSlug);

        // Assert
        Assert.Equal(ManagerSeedData.ProjectTitle, project.Title);
        Assert.Equal(ManagerSeedData.ProjectSlug.Text, project.Slug.Text);
    }

    [Theory]
    [MemberData(nameof(ProjectBySlugReaders))]
    public void ProjectBySlugReaders_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException(string _, Func<IProjectManager, Slug, Project> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var unknownSlug = Slug.FromName("Unknown Project");

        // Act
        var act = () => readMethod(projectManager, unknownSlug);

        // Assert
        Assert.Throws<ProjectNotFoundException>(act);
    }

    [Theory]
    [MemberData(nameof(AllProjectReaders))]
    public void AllProjectReaders_WhenSeededDataExists_ShouldReturnProjects(string _, Func<IProjectManager, IReadOnlyCollection<Project>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var projects = readMethod(projectManager);

        // Assert
        Assert.Contains(projects, project => project.Slug.Text == ManagerSeedData.ProjectSlug.Text);
    }

    [Theory]
    [MemberData(nameof(WorkspaceProjectReaders))]
    public void WorkspaceProjectReaders_WhenWorkspaceExists_ShouldReturnProjects(string _, Func<IProjectManager, int, IReadOnlyCollection<Project>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var workspaceId = dbContext.Workspaces.Single(workspace => workspace.Slug == ManagerSeedData.WorkspaceSlug).Id;

        // Act
        var projects = readMethod(projectManager, workspaceId);

        // Assert
        Assert.Contains(projects, project => project.Slug.Text == ManagerSeedData.ProjectSlug.Text);
    }

    [Theory]
    [MemberData(nameof(WorkspaceProjectReaders))]
    public void WorkspaceProjectReaders_WhenWorkspaceDoesNotExist_ShouldReturnEmpty(string _, Func<IProjectManager, int, IReadOnlyCollection<Project>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var projects = readMethod(projectManager, -9999);

        // Assert
        Assert.Empty(projects);
    }

    [Fact]
    public void GetTopicsFromProjectByProjectId_ShouldReturnTopicsForProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var topics = projectManager.GetTopicsFromProjectByProjectId(projectId);

        // Assert
        Assert.NotEmpty(topics);
        Assert.Contains(topics, topic => topic.Name == "Studiedruk en evaluatie");
    }

    [Fact]
    public void GetTopicsFromProjectByProjectId_WhenProjectDoesNotExist_ShouldReturnEmpty()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var topics = projectManager.GetTopicsFromProjectByProjectId(-9999);

        // Assert
        Assert.Empty(topics);
    }

    [Fact]
    public void GetYouthsFromProjectByProjectId_ShouldReturnYouthsForProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var youths = projectManager.GetYouthsFromProjectByProjectId(projectId);

        // Assert
        Assert.NotEmpty(youths);
        Assert.Contains(youths, youth => youth.Token == ManagerSeedData.YouthToken);
    }

    [Fact]
    public void GetYouthsFromProjectByProjectId_WhenProjectDoesNotExist_ShouldReturnEmpty()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var youths = projectManager.GetYouthsFromProjectByProjectId(-9999);

        // Assert
        Assert.Empty(youths);
    }

    [Fact]
    public void GetTopicById_WhenTopicExists_ShouldReturnTopic()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var topicId = dbContext.Topics.Select(topic => topic.Id).First();

        // Act
        var topic = projectManager.GetTopicById(topicId);

        // Assert
        Assert.Equal(topicId, topic.Id);
    }

    [Fact]
    public void GetTopicById_WhenTopicDoesNotExist_ShouldThrowTopicNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var act = () => projectManager.GetTopicById(-9999);

        // Assert
        Assert.Throws<TopicNotFoundException>(act);
    }

    [Fact]
    public void GetYouthByToken_WhenYouthExists_ShouldReturnYouth()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var youth = projectManager.GetYouthByToken(ManagerSeedData.YouthToken);

        // Assert
        Assert.Equal(ManagerSeedData.YouthToken, youth.Token);
    }

    [Fact]
    public void GetYouthByToken_WhenYouthDoesNotExist_ShouldThrowYouthNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var act = () => projectManager.GetYouthByToken("unknown-youth");

        // Assert
        Assert.Throws<YouthNotFoundException>(act);
    }

    #endregion

    #region MutationsAndNegativePaths

    [Fact]
    public void AddProject_WhenTitleIsUnique_ShouldCreateProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var existingProject = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);
        var title = $"Integration Project {Guid.NewGuid():N}";

        // Act
        var createdProject = projectManager.AddProject(
            title,
            "unused-because-manager-builds-slug-from-title",
            "Integration test project",
            Status.Draft,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(30),
            InteractionType.Chat,
            existingProject.Workspace.Id);

        // Assert
        Assert.True(createdProject.Id > 0);
        Assert.Equal(Slug.FromName(title).Text, createdProject.Slug.Text);
    }

    [Fact]
    public void AddProject_WhenSlugAlreadyExists_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var existingProject = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);

        // Act
        var act = () => projectManager.AddProject(
            ManagerSeedData.ProjectTitle,
            "ignored",
            "duplicate",
            Status.Active,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(1),
            InteractionType.Chat,
            existingProject.Workspace.Id);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void AddTopic_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var act = () => projectManager.AddTopic("New topic", "context", -9999);

        // Assert
        Assert.Throws<ProjectNotFoundException>(act);
    }

    [Fact]
    public void AddTopic_WhenProjectExists_ShouldCreateTopic()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var topic = projectManager.AddTopic("Integration topic", "Integration context", projectId);

        // Assert
        Assert.True(topic.Id > 0);
        Assert.Equal("Integration topic", topic.Name);
    }

    [Fact]
    public void ChangeProject_WhenProjectIsValid_ShouldPersistChanges()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var project = projectManager.GetProjectBySlugWithWorkspaceAndQuestions(ManagerSeedData.ProjectSlug);
        project.Description = "Updated project description";

        // Act
        var updatedProject = projectManager.ChangeProject(project);

        // Assert
        Assert.Equal("Updated project description", updatedProject.Description);
    }

    [Fact]
    public void ChangeProject_WhenProjectIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var project = projectManager.GetProjectBySlug(ManagerSeedData.ProjectSlug);
        project.Title = new string('t', 150);

        // Act
        var act = () => projectManager.ChangeProject(project);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void AddYouth_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var act = () => projectManager.AddYouth("integration-token", "integration@example.com", -9999);

        // Assert
        Assert.Throws<ProjectNotFoundException>(act);
    }

    [Fact]
    public void AddYouth_WhenProjectExists_ShouldCreateYouth()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var youth = projectManager.AddYouth($"integration-{Guid.NewGuid():N}", "integration@example.com", projectId);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(youth.Token));
    }

    [Fact]
    public void ChangeTopic_WhenTopicIsValid_ShouldPersistChanges()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var topic = projectManager.GetTopicsFromProjectByProjectId(projectManager.GetProjectBySlug(ManagerSeedData.ProjectSlug).Id).First();
        topic.Context = "Updated context";

        // Act
        var changedTopic = projectManager.ChangeTopic(topic);

        // Assert
        Assert.Equal("Updated context", changedTopic.Context);
    }

    [Fact]
    public void RemoveTopic_WhenTopicDoesNotExist_ShouldThrowTopicNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var act = () => projectManager.RemoveTopic(-9999);

        // Assert
        Assert.Throws<TopicNotFoundException>(act);
    }

    [Fact]
    public void ChangeYouth_WhenYouthIsValid_ShouldPersistChanges()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var youth = projectManager.GetYouthByToken(ManagerSeedData.YouthToken);
        youth.Email = "updated@student.nova.be";

        // Act
        var changedYouth = projectManager.ChangeYouth(youth);

        // Assert
        Assert.Equal("updated@student.nova.be", changedYouth.Email);
    }

    [Fact]
    public void ChangeYouth_WhenYouthIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var youth = projectManager.GetYouthByToken(ManagerSeedData.YouthToken);
        youth.Token = string.Empty;

        // Act
        var act = () => projectManager.ChangeYouth(youth);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void RemoveYouth_WhenYouthDoesNotExist_ShouldThrowYouthNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var act = () => projectManager.RemoveYouth("unknown-youth");

        // Assert
        Assert.Throws<YouthNotFoundException>(act);
    }

    [Fact]
    public void RemoveYouth_WhenYouthExists_ShouldDeleteYouth()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
        var youth = projectManager.AddYouth($"temp-{Guid.NewGuid():N}", "temp@student.nova.be", projectId);

        // Act
        projectManager.RemoveYouth(youth.Token);

        // Assert
        Assert.Throws<YouthNotFoundException>(() => projectManager.GetYouthByToken(youth.Token));
    }

    [Fact]
    public void RemoveProject_WhenProjectExists_ShouldDeleteProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var workspaceId = dbContext.Workspaces.Single(workspace => workspace.Slug == ManagerSeedData.WorkspaceSlug).Id;
        var title = $"Project to remove {Guid.NewGuid():N}";
        var project = projectManager.AddProject(title, "ignored", "temp", Status.Draft, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), InteractionType.Chat, workspaceId);

        // Act
        projectManager.RemoveProject(project.Id);

        // Assert
        Assert.Throws<ProjectNotFoundException>(() => projectManager.GetProjectById(project.Id));
    }

    [Fact]
    public void RemoveProject_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var act = () => projectManager.RemoveProject(-9999);

        // Assert
        Assert.Throws<ProjectNotFoundException>(act);
    }

    #endregion
}



