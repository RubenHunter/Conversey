using Conversey.BL.Subplatform;
using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
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

    #region GetWorkspace

    [Fact]
    public void GetWorkspaceBySlug_ShouldReturnWorkspace()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        // Act
        var workspace = workspaceManager.GetWorkspaceBySlug(ManagerSeedData.WorkspaceSlug);

        // Assert
        Assert.Equal(ManagerSeedData.WorkspaceName, workspace.Name);
        Assert.Equal(ManagerSeedData.WorkspaceSlug.Text, workspace.Slug.Text);
    }

    [Fact]
    public void GetWorkspaceBySlug_WhenWorkspaceDoesNotExist_ShouldThrowWorkspaceNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var unknownSlug = Conversey.BL.Domain.Common.Slug.FromName("Unknown Workspace");

        // Act
        var act = () => workspaceManager.GetWorkspaceBySlug(unknownSlug);

        // Assert
        Assert.Throws<WorkspaceNotFoundException>(act);
    }

    [Fact]
    public void GetWorkspaceBySlugWithProjects_ShouldReturnWorkspaceWithItsProjects()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        // Act
        var workspace = workspaceManager.GetWorkspaceBySlugWithProjects(ManagerSeedData.WorkspaceSlug);

        // Assert
        Assert.Equal(ManagerSeedData.WorkspaceName, workspace.Name);
        Assert.Contains(workspace.Projects, project => project.Slug.Text == ManagerSeedData.ProjectSlug.Text);
    }

    [Fact]
    public void GetWorkspaceById_ShouldReturnWorkspace()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var workspaceBySlug = workspaceManager.GetWorkspaceBySlug(ManagerSeedData.WorkspaceSlug);

        // Act
        var workspace = workspaceManager.GetWorkspaceById(workspaceBySlug.Id);

        // Assert
        Assert.Equal(workspaceBySlug.Id, workspace.Id);
        Assert.Equal(ManagerSeedData.WorkspaceSlug.Text, workspace.Slug.Text);
    }

    [Fact]
    public void GetWorkspaceById_WhenWorkspaceDoesNotExist_ShouldThrowWorkspaceNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        // Act
        var act = () => workspaceManager.GetWorkspaceById(-9999);

        // Assert
        Assert.Throws<WorkspaceNotFoundException>(act);
    }

    [Fact]
    public void GetAllWorkspacesWithProjects_ShouldContainSeededWorkspaceAndProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        // Act
        var workspaces = workspaceManager.GetAllWorkspacesWithProjects();

        // Assert
        Assert.Contains(workspaces, workspace =>
            workspace.Slug.Text == ManagerSeedData.WorkspaceSlug.Text &&
            workspace.Projects.Any(project => project.Slug.Text == ManagerSeedData.ProjectSlug.Text));
    }

    [Fact]
    public void GetAllWorkspaces_ShouldContainSeededWorkspace()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        // Act
        var workspaces = workspaceManager.GetAllWorkspaces();

        // Assert
        Assert.Contains(workspaces, workspace => workspace.Slug.Text == ManagerSeedData.WorkspaceSlug.Text);
    }

    [Fact]
    public void GetWorkspaceByIdWithProjects_ShouldReturnWorkspaceWithProjects()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var workspaceBySlug = workspaceManager.GetWorkspaceBySlug(ManagerSeedData.WorkspaceSlug);

        // Act
        var workspace = workspaceManager.GetWorkspaceByIdWithProjects(workspaceBySlug.Id);

        // Assert
        Assert.Equal(workspaceBySlug.Id, workspace.Id);
        Assert.NotEmpty(workspace.Projects);
    }

    [Fact]
    public void GetWorkspaceByIdWithProjects_WhenWorkspaceDoesNotExist_ShouldThrowWorkspaceNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        // Act
        var act = () => workspaceManager.GetWorkspaceByIdWithProjects(-9999);

        // Assert
        Assert.Throws<WorkspaceNotFoundException>(act);
    }

    #endregion

    #region CreateWorkspace

    [Fact]
    public void CreateWorkspace_WhenSlugIsUnique_ShouldPersistWorkspace()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var name = $"Integration Workspace {Guid.NewGuid():N}";
        var slug = Conversey.BL.Domain.Common.Slug.FromName(name);

        // Act
        var createdWorkspace = workspaceManager.CreateWorkspace(name, slug);

        // Assert
        Assert.True(createdWorkspace.Id > 0);
        Assert.Equal(slug.Text, createdWorkspace.Slug.Text);
    }

    [Fact]
    public void CreateWorkspace_WhenSlugAlreadyExists_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();

        // Act
        var act = () => workspaceManager.CreateWorkspace("Different name", ManagerSeedData.WorkspaceSlug);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    #endregion
}


