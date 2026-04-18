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
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        var workspace = workspaceManager.GetWorkspaceById(ManagerSeedData.WorkspaceSlug);
        var project = manager.GetProjectById(workspace.Id, ManagerSeedData.ProjectSlug);

        Assert.Equal(ManagerSeedData.ProjectSlug.Text, project.Id.Text);
        Assert.Equal(ManagerSeedData.ProjectName, project.Name);
    }

    [Fact]
    public void GetYouthByToken_ShouldReturnSeededYouth()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        var workspace = workspaceManager.GetWorkspaceById(ManagerSeedData.WorkspaceSlug);
        var project = manager.GetProjectById(workspace.Id, ManagerSeedData.ProjectSlug);
        var youth = manager.GetYouth(project, ManagerSeedData.YouthToken);

        Assert.Equal(ManagerSeedData.YouthToken, youth.Id);
    }

    [Fact]
    public void AddYouth_WithUniqueToken_ShouldPersistYouth()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var token = Guid.NewGuid();

        var created = manager.AddYouth(token, $"integration-{Guid.NewGuid():N}@example.com", ManagerSeedData.ProjectSlug);
        var workspace = workspaceManager.GetWorkspaceById(ManagerSeedData.WorkspaceSlug);
        var project = manager.GetProjectById(workspace.Id, ManagerSeedData.ProjectSlug);

        Assert.Equal(token, created.Id);

        var loaded = manager.GetYouth(project, token);
        Assert.Equal(token, loaded.Id);
    }
}
