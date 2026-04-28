using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Tests.IntegrationTests.Infrastructure;

namespace Tests.IntegrationTests;

public class WorkspaceManagerIntegrationTests : IClassFixture<ManagerIntegrationTestFixture>
{
    private readonly ManagerIntegrationTestFixture _fixture;

    public WorkspaceManagerIntegrationTests(ManagerIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetWorkspaceBySlug_ShouldReturnSeededWorkspace()
    {
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        var workspace = workspaceManager.GetWorkspaceById(ManagerSeedData.WorkspaceSlug);

        Assert.Equal(ManagerSeedData.WorkspaceName, workspace.Name);
        Assert.Equal(ManagerSeedData.WorkspaceSlug.Text, workspace.Id.Text);
    }

    [Fact]
    public void GetWorkspaceById_ShouldReturnSeededWorkspace()
    {
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        var workspace = workspaceManager.GetWorkspaceById(ManagerSeedData.WorkspaceSlug);

        Assert.Equal(ManagerSeedData.WorkspaceSlug.Text, workspace.Id.Text);
        Assert.Equal(ManagerSeedData.WorkspaceName, workspace.Name);
    }

    [Fact]
    public void CreateWorkspace_WhenSlugAlreadyExists_ShouldThrowValidationException()
    {
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        var act = () => workspaceManager.AddWorkspace(ManagerSeedData.WorkspaceName);

        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void CreateWorkspace_WithUniqueSlug_ShouldPersistWorkspace()
    {
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var name = $"Integration Workspace {Guid.NewGuid():N}";

        var created = workspaceManager.AddWorkspace(name);

        Assert.Equal(Slug.FromName(name).Text, created.Id.Text);
        Assert.Equal(name, created.Name);
    }
}
