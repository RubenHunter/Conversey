using Conversey.BL.Subplatform.Survey;
using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey;
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

    #region GetProject

    [Fact]
    public void GetProjectBySlug_ShouldReturnProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var project = projectManager.GetProjectBySlug(ManagerSeedData.ProjectSlug);

        // Assert
        Assert.Equal(ManagerSeedData.ProjectTitle, project.Title);
        Assert.Equal(ManagerSeedData.ProjectSlug.Text, project.Slug.Text);
    }

    [Fact]
    public void GetProjectBySlug_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var unknownSlug = Slug.FromName("Unknown Project");

        // Act
        var act = () => projectManager.GetProjectBySlug(unknownSlug);

        // Assert
        Assert.Throws<ProjectNotFoundException>(act);
    }

    [Fact]
    public void GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions_ShouldReturnProjectFromWorkspace()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        // Act
        var project = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);

        // Assert
        Assert.Equal(ManagerSeedData.ProjectTitle, project.Title);
        Assert.Equal(ManagerSeedData.WorkspaceSlug.Text, project.Workspace.Slug.Text);
        Assert.NotEmpty(project.Topic);
        Assert.NotEmpty(project.Youths);
        Assert.NotEmpty(project.Questions);
    }

    [Fact]
    public void GetProjectsFromWorkspaceByWorkspaceId_ShouldReturnProjectsForThatWorkspace()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var project = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);

        // Act
        var projects = projectManager.GetProjectsFromWorkspaceByWorkspaceId(project.Workspace.Id);

        // Assert
        Assert.Contains(projects, p => p.Slug.Text == ManagerSeedData.ProjectSlug.Text);
    }

    [Fact]
    public void GetTopicsFromProjectByProjectId_ShouldReturnTopicsForProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var project = projectManager.GetProjectBySlug(ManagerSeedData.ProjectSlug);

        // Act
        var topics = projectManager.GetTopicsFromProjectByProjectId(project.Id);

        // Assert
        Assert.NotEmpty(topics);
        Assert.Contains(topics, topic => topic.Name == "Studiedruk en evaluatie");
    }

    [Fact]
    public void GetYouthsFromProjectByProjectId_ShouldReturnYouthsForProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var project = projectManager.GetProjectBySlug(ManagerSeedData.ProjectSlug);

        // Act
        var youths = projectManager.GetYouthsFromProjectByProjectId(project.Id);

        // Assert
        Assert.NotEmpty(youths);
        Assert.Contains(youths, youth => youth.Token == ManagerSeedData.YouthToken);
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



