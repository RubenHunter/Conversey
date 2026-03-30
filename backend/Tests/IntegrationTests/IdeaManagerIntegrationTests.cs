using Conversey.BL.Subplatform.Survey.Ideation;
using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.BL.Subplatform.Survey;
using Conversey.DAL;
using Microsoft.Extensions.DependencyInjection;
using Tests.IntegrationTests.Infrastructure;

namespace Tests.IntegrationTests;

public class IdeaManagerIntegrationTests : IClassFixture<ManagerIntegrationTestFixture>
{
    private readonly ManagerIntegrationTestFixture _fixture;

    public IdeaManagerIntegrationTests(ManagerIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region IdeaQueries

    [Fact]
    public void GetIdeaById_WhenIdeaExists_ShouldReturnIdea()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();

        // Act
        var idea = ideaManager.GetIdeaById(ideaId);

        // Assert
        Assert.Equal(ideaId, idea.Id);
        Assert.False(string.IsNullOrWhiteSpace(idea.Content));
    }

    [Fact]
    public void GetIdeaById_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.GetIdeaById(-9999);

        // Assert
        Assert.Throws<IdeaNotFoundException>(act);
    }

    [Fact]
    public void GetIdeaByIdWithProject_WhenIdeaExists_ShouldReturnIdeaWithProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();

        // Act
        var idea = ideaManager.GetIdeaByIdWithProject(ideaId);

        // Assert
        Assert.NotNull(idea.Project);
        Assert.NotNull(idea.Topic);
        Assert.NotNull(idea.Youth);
    }

    [Fact]
    public void GetIdeasFromTopicByProjectSlugAndTopicId_ShouldReturnApprovedIdeasForThatTopic()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var topicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var ideas = ideaManager.GetIdeasFromTopicByProjectSlugAndTopicId(ManagerSeedData.ProjectSlug, topicId);

        // Assert
        Assert.NotEmpty(ideas);
        Assert.True(ideas.SequenceEqual(ideas.OrderByDescending(i => i.SubmissionDate).ThenByDescending(i => i.Id)));
        Assert.All(ideas, idea =>
        {
            Assert.Equal(IdeaStatus.Approved, idea.Status);
            Assert.NotNull(idea.Youth);
        });
    }

    [Fact]
    public void GetIdeasFromProjectByYouthToken_ShouldReturnOnlyIdeasFromThatYouthOrderedDescending()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var ideas = ideaManager.GetIdeasFromProjectByYouthToken(projectId, ManagerSeedData.YouthToken);

        // Assert
        Assert.NotEmpty(ideas);
        Assert.All(ideas, idea => Assert.Equal(ManagerSeedData.YouthToken, idea.Youth.Token));
        Assert.True(ideas.SequenceEqual(ideas.OrderByDescending(i => i.SubmissionDate).ThenByDescending(i => i.Id)));
    }

    #endregion

    #region SubmitIdea

    [Fact]
    public void SubmitIdea_WhenPayloadIsValid_ShouldReturnApprovedSubmission()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
        var topicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var response = ideaManager.SubmitIdea("Integration idea", projectId, topicId, ManagerSeedData.YouthToken);

        // Assert
        var approved = Assert.IsType<SubmissionResponse.Approved>(response);
        Assert.Equal(IdeaStatus.Approved, approved.idea.Status);
        Assert.Equal(ManagerSeedData.YouthToken, approved.idea.Youth.Token);
    }

    [Fact]
    public void SubmitIdea_WhenProjectDoesNotExist_ShouldThrowProjectNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.SubmitIdea("text", -9999, -9999, ManagerSeedData.YouthToken);

        // Assert
        Assert.Throws<ProjectNotFoundException>(act);
    }

    [Fact]
    public void SubmitIdea_WhenTopicDoesNotExistInProject_ShouldThrowTopicNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var act = () => ideaManager.SubmitIdea("text", projectId, -9999, ManagerSeedData.YouthToken);

        // Assert
        Assert.Throws<TopicNotFoundException>(act);
    }

    [Fact]
    public void SubmitIdea_WhenYouthDoesNotExist_ShouldThrowYouthNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
        var topicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var act = () => ideaManager.SubmitIdea("text", projectId, topicId, "unknown-youth-token");

        // Assert
        Assert.Throws<YouthNotFoundException>(act);
    }

    [Fact]
    public void SubmitIdea_WhenContentIsWhitespace_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
        var topicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var act = () => ideaManager.SubmitIdea("   ", projectId, topicId, ManagerSeedData.YouthToken);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    #endregion

    #region ReactionsAndNegativePaths

    [Fact]
    public void AddIdeaReaction_WhenReactionAlreadyExists_ShouldReturnExistingReaction()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();
        var firstReaction = ideaManager.AddIdeaReaction("👍", ideaId, ManagerSeedData.YouthToken);

        // Act
        var reaction = ideaManager.AddIdeaReaction(firstReaction.Emoji, firstReaction.IdeaId, firstReaction.YouthToken);

        // Assert
        Assert.Equal(firstReaction.Id, reaction.Id);
        Assert.Equal(firstReaction.Emoji, reaction.Emoji);
        Assert.Equal(1, dbContext.IdeaReactions.Count(ir => ir.IdeaId == ideaId && ir.YouthToken == ManagerSeedData.YouthToken && ir.Emoji == "👍"));
    }

    [Fact]
    public void GetIdeaReactionsFromIdeaByIdeaId_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.GetIdeaReactionsFromIdeaByIdeaId(-9999);

        // Assert
        Assert.Throws<IdeaNotFoundException>(act);
    }

    [Fact]
    public void RemoveIdeaReaction_WhenReactionDoesNotExist_ShouldThrowIdeaReactionNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.RemoveIdeaReaction(-9999, ManagerSeedData.YouthToken, ":)");

        // Assert
        Assert.Throws<IdeaReactionNotFoundException>(act);
    }

    [Fact]
    public void GetResponseById_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.GetResponseById(-9999);

        // Assert
        Assert.Throws<ResponseNotFoundException>(act);
    }

    [Fact]
    public void RemoveResponseReaction_WhenReactionDoesNotExist_ShouldThrowResponseReactionNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.RemoveResponseReaction(-9999, ManagerSeedData.YouthToken, ":)");

        // Assert
        Assert.Throws<ResponseReactionNotFoundException>(act);
    }

    #endregion
}





