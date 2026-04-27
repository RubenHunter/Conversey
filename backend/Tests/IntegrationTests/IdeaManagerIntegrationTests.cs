using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Ideation;
using Conversey.DAL;
using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public void GetIdeasByProjectIdAndTopicId_ShouldReturnSeededIdeas()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var project = projectManager.GetProjectById(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug);
        var topicId = dbContext.Topics
            .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
            .Select(topic => topic.Id)
            .First();

        var ideas = manager.GetIdeasByProjectIdAndTopicId(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId).ToList();

        Assert.True(ideas.Count > 0);
        Assert.All(ideas, idea => Assert.NotEmpty(idea.SemanticCategories));
        var loadedIdea = manager.GetIdeaByIdWithProjectAndResponses(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ideas[0].Id);
        Assert.Equal(project.Id.Text, loadedIdea.Project.Id.Text);
        Assert.Equal(topicId, loadedIdea.Topic.Id);
    }

    [Fact]
    public void SubmitIdea_WhenAllowed_ShouldReturnApproved()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var topicId = dbContext.Topics
            .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
            .Select(topic => topic.Id)
            .First();

        var response = manager.SubmitIdea(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken, "Integration idea");

        var approved = Assert.IsType<SubmissionResponse.Approved>(response);
        Assert.Equal(ModerationStatus.Approved, approved.Idea.Status);
        Assert.NotEmpty(approved.Idea.SemanticCategories);
    }

    [Fact]
    public void SubmitIdea_WhenCategorizationFails_ShouldStillPersistIdea()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        _fixture.SetAiCategorizationBehavior(throwOnCategorize: true);

        try
        {
            using var scope = _fixture.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
            var topicId = dbContext.Topics
                .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
                .Select(topic => topic.Id)
                .First();

            var response = manager.SubmitIdea(
                ManagerSeedData.WorkspaceSlug,
                ManagerSeedData.ProjectSlug,
                topicId,
                Guid.NewGuid(),
                "Idea that should survive categorization failure");

            var approved = Assert.IsType<SubmissionResponse.Approved>(response);
            Assert.Equal(ModerationStatus.Approved, approved.Idea.Status);
            Assert.Equal(new[] { "General ideas" }, approved.Idea.SemanticCategories);
        }
        finally
        {
            _fixture.SetAiCategorizationBehavior(throwOnCategorize: false);
        }
    }

    [Fact]
    public void SubmitIdea_WhenTopicAlreadyHasACategory_ShouldReuseTheExistingLabel()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var topicId = dbContext.Topics
            .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
            .Select(topic => topic.Id)
            .First();

        var response = manager.SubmitIdea(
            ManagerSeedData.WorkspaceSlug,
            ManagerSeedData.ProjectSlug,
            topicId,
            ManagerSeedData.YouthToken,
            "Deadline pressure and exams are overwhelming.") as SubmissionResponse.Approved;

        Assert.NotNull(response);
        Assert.Contains("Study pressure", response.Idea.SemanticCategories);
    }

    [Fact]
    public void SubmitIdea_WhenFlagged_ShouldReturnPendingWithSuggestion()
    {
        _fixture.SetAiModerationBehavior(isAllowed: false, alternative: "rewrite please");
        try
        {
            using var scope = _fixture.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
            var topicId = dbContext.Topics
                .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
                    .Select(topic => topic.Id)
                    .First();

            var response = manager.SubmitIdea(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken, "flagged");

            var pending = Assert.IsType<SubmissionResponse.Pending>(response);
            Assert.Equal(ModerationStatus.Pending, pending.Idea.Status);
            Assert.Equal("rewrite please", pending.Decision.Suggestion);
        }
        finally
        {
            _fixture.SetAiModerationBehavior(isAllowed: true);
        }
    }

    [Fact]
    public void AddResponse_And_Reactions_ShouldPersist()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var topicId = dbContext.Topics
            .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
            .Select(topic => topic.Id)
            .First();

        var idea = Assert.IsType<SubmissionResponse.Approved>(
            manager.SubmitIdea(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken, "response test idea")).Idea;
        var responseSubmission = Assert.IsType<ResponseSubmissionResponse.Approved>(manager.AddResponse(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, idea.Id, ManagerSeedData.YouthToken, "response"));

        _ = manager.AddIdeaReaction(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, idea.Id, ManagerSeedData.YouthToken, "like");
        _ = manager.AddResponseReaction(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, idea.Id, responseSubmission.IdeaResponse.Id, ManagerSeedData.YouthToken, "upvote");

        var ideaReactions = manager.GetIdeaReactionsByIdeaId(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, idea.Id);
        var responseReactions = manager.GetResponseReactionsByResponseId(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, idea.Id, responseSubmission.IdeaResponse.Id);

        Assert.Contains(ideaReactions, r => r.Emoji == "like");
        Assert.Contains(responseReactions, r => r.Emoji == "upvote");
    }
}
