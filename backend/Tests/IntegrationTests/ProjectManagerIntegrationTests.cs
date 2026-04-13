using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Subplatform.Survey;
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

    [Fact]
    public void GetProjectBySlug_ShouldReturnSeededProject()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        var project = manager.GetProjectBySlug(ManagerSeedData.ProjectSlug);

        Assert.Equal(ManagerSeedData.ProjectSlug.Text, project.Slug.Text);
        Assert.Equal(ManagerSeedData.ProjectName, project.Name);
    }

    [Fact]
    public void AddProject_WithUniqueTitle_ShouldPersistProject()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var title = $"Integration Project {Guid.NewGuid():N}";

        var created = manager.AddProject(
            title,
            slug: "ignored",
            description: "Integration",
            status: Status.Active,
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddDays(7),
            interactionForm: InteractionType.Chat,
            workspaceSlug: ManagerSeedData.WorkspaceSlug);

        Assert.Equal(Slug.FromName(title).Text, created.Slug.Text);
    }

    [Fact]
    public void AddProject_WhenSlugAlreadyExists_ShouldThrowValidationException()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        var act = () => manager.AddProject(
            ManagerSeedData.ProjectName,
            slug: "ignored",
            description: "Duplicate",
            status: Status.Active,
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddDays(1),
            interactionForm: InteractionType.Chat,
            workspaceSlug: ManagerSeedData.WorkspaceSlug);

        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void AddTopic_And_GetTopics_ShouldWorkForProjectSlug()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        _ = manager.AddTopic("New topic", "context", ManagerSeedData.ProjectSlug);
        var topics = manager.GetTopicsFromProjectByProjectId(ManagerSeedData.ProjectSlug);

        Assert.Contains(topics, t => t.Name == "New topic");
    }

    [Fact]
    public void AddAndRemoveYouth_ShouldWorkWithGuidToken()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var token = Guid.NewGuid();

        var youth = manager.AddYouth(token, "youth@example.com", ManagerSeedData.ProjectSlug);
        manager.RemoveYouth(token);

        Assert.Equal(token, youth.Token);
        Assert.Throws<YouthNotFoundException>(() => manager.GetYouthByToken(token));
    }
}
