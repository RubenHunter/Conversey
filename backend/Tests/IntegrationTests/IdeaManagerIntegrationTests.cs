using Conversey.BL.Domain.Ideation;
using Conversey.BL.Subplatform.Survey.Ideation;
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
    public void GetAllIdeas_ShouldReturnSeededIdeas()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        var ideas = manager.GetAllIdeas();

        Assert.NotEmpty(ideas);
    }

    [Fact]
    public void SubmitIdea_WhenAllowed_ShouldReturnApproved()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<Conversey.BL.Subplatform.Survey.IProjectManager>();
        var topicId = projectManager.GetTopicsFromProjectByProjectId(ManagerSeedData.ProjectSlug).First().Id;

        var response = manager.SubmitIdea("Integration idea", ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken);

        var approved = Assert.IsType<SubmissionResponse.Approved>(response);
        Assert.Equal(ModerationStatus.Approved, approved.idea.Status);
    }

    [Fact]
    public void SubmitIdea_WhenFlagged_ShouldReturnPendingWithSuggestion()
    {
        _fixture.SetAiModerationBehavior(isAllowed: false, alternative: "rewrite please");
        try
        {
            using var scope = _fixture.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
            var projectManager = scope.ServiceProvider.GetRequiredService<Conversey.BL.Subplatform.Survey.IProjectManager>();
            var topicId = projectManager.GetTopicsFromProjectByProjectId(ManagerSeedData.ProjectSlug).First().Id;

            var response = manager.SubmitIdea("flagged", ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken);

            var pending = Assert.IsType<SubmissionResponse.Pending>(response);
            Assert.Equal(ModerationStatus.Pending, pending.idea.Status);
            Assert.Equal("rewrite please", pending.decision.Suggestion);
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

        var idea = manager.GetAllIdeas().First();
        var responseSubmission = Assert.IsType<ResponseSubmissionResponse.Approved>(manager.AddResponse("response", idea.Id, ManagerSeedData.YouthToken));

        _ = manager.AddIdeaReaction("like", idea.Id, ManagerSeedData.YouthToken);
        _ = manager.AddResponseReaction("upvote", responseSubmission.Response.Id, ManagerSeedData.YouthToken);

        var ideaReactions = manager.GetIdeaReactionsFromIdeaByIdeaId(idea.Id);
        var responseReactions = manager.GetResponseReactionsFromResponseByResponseId(responseSubmission.Response.Id);

        Assert.Contains(ideaReactions, r => r.Emoji == "like");
        Assert.Contains(responseReactions, r => r.Emoji == "upvote");
    }
}
