using Conversey.BL.Administration;
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
    public void GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions_ShouldReturnSeededProjectWithRelations()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        var project = manager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);

        Assert.Equal(ManagerSeedData.ProjectSlug.Text, project.Id.Text);
        Assert.Equal(ManagerSeedData.ProjectName, project.Name);
        Assert.Equal(ManagerSeedData.WorkspaceSlug.Text, project.Workspace.Id.Text);
        Assert.NotEmpty(project.Topic);
        Assert.NotEmpty(project.Questions);
        Assert.NotEmpty(project.Youth);
    }

    [Fact]
    public void GetYouthByToken_ShouldReturnSeededYouth()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        var youth = manager.GetYouthByToken(ManagerSeedData.YouthToken);
        var project = manager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);

        Assert.Equal(ManagerSeedData.YouthToken, youth.Id);
        Assert.Contains(project.Youth, y => y.Id == youth.Id);
    }

    [Fact]
    public void AddYouth_WithUniqueToken_ShouldPersistYouth()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var token = Guid.NewGuid();

        var created = manager.AddYouth(token, $"integration-{Guid.NewGuid():N}@example.com", ManagerSeedData.ProjectSlug);
        var project = manager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);

        Assert.Equal(token, created.Id);

        var loaded = manager.GetYouthByToken(token);
        Assert.Equal(token, loaded.Id);
        Assert.Contains(project.Youth, y => y.Id == loaded.Id);
    }
}
