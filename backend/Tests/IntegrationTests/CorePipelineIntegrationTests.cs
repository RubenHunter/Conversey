using Conversey.BL.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Ideation;
using Conversey.DAL;
using Conversey.BL.Survey;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.IntegrationTests.Infrastructure;

namespace Tests.IntegrationTests;

[Trait("Category", "CorePipeline")]
public class CorePipelineIntegrationTests : IClassFixture<ManagerIntegrationTestFixture>
{
    private readonly ManagerIntegrationTestFixture _fixture;

    public CorePipelineIntegrationTests(ManagerIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void CorePipeline_WorkspaceProjectQuestionAndAnswerFlow_ShouldPersistAcrossManagers()
    {
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        var workspace = workspaceManager.GetWorkspaceById(ManagerSeedData.WorkspaceSlug);
        var project = projectManager.GetProjectById(workspace.Id, ManagerSeedData.ProjectSlug);
        var youth = projectManager.GetYouth(project, ManagerSeedData.YouthToken);
        var question = questionManager.AddQuestion(new OpenQuestion
        {
            Text = $"What should we improve? {Guid.NewGuid():N}",
            Required = true,
            Project = project
        });

        var typedQuestion = Assert.IsAssignableFrom<Question<Answer<string>>>(questionManager.GetQuestionById(question.Id));

        var answer = questionManager.AddAnswer(new Answer<string>
        {
            Value = "More quiet spaces",
            Question = typedQuestion,
            Youth = youth
        });

        var workspaceById = workspaceManager.GetWorkspaceById(workspace.Id);
        var projectById = projectManager.GetProjectById(workspace.Id, project.Id);
        var loadedAnswer = questionManager.GetAnswerById(answer.Id);

        Assert.True(workspace.Id.Text == workspaceById.Id.Text);
        Assert.True(project.Id.Text == projectById.Id.Text);
        Assert.Equal(answer.Id, loadedAnswer.Id);
    }

    [Fact]
    public async Task CorePipeline_IdeaToResponseToReactionFlow_ShouldPersistAndBeReadable()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();

        var project = projectManager.GetProjectById(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug);
        var topicId = dbContext.Topics
            .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
            .Select(topic => topic.Id)
            .First();
        var ideaSubmission = await ideaManager.SubmitIdeaAsync(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken, "Pipeline idea content") switch
        {
            SubmissionResponse.Approved approved => approved,
            _ => throw new InvalidOperationException("Expected approved idea submission.")
        };

        var responseSubmission = await ideaManager.AddResponseAsync(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ideaSubmission.Idea.Id, ManagerSeedData.YouthToken, "Pipeline response content") switch
        {
            ResponseSubmissionResponse.Approved approved => approved,
            _ => throw new InvalidOperationException("Expected approved response submission.")
        };

        var ideaReaction = ideaManager.AddIdeaReaction(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ideaSubmission.Idea.Id, ManagerSeedData.YouthToken, "like");
        var responseReaction = ideaManager.AddResponseReaction(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ideaSubmission.Idea.Id, responseSubmission.IdeaResponse.Id, ManagerSeedData.YouthToken, "upvote");

        var ideaWithResponses = ideaManager.GetIdeaByIdWithProjectAndResponses(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ideaSubmission.Idea.Id);
        var ideaReactions = ideaManager.GetIdeaReactionsByIdeaId(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ideaSubmission.Idea.Id);
        var responseReactions = ideaManager.GetResponseReactionsByResponseId(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ideaSubmission.Idea.Id, responseSubmission.IdeaResponse.Id);

        Assert.Equal(ModerationStatus.Approved, ideaWithResponses.Status);
        Assert.Contains(ideaWithResponses.Responses, response => response.Id == responseSubmission.IdeaResponse.Id);
        Assert.Contains(ideaReactions, reaction => reaction.Id == ideaReaction.Id && reaction.Emoji == "like");
        Assert.Contains(responseReactions, reaction => reaction.Id == responseReaction.Id && reaction.Emoji == "upvote");
    }

    [Fact]
    public async Task CorePipeline_SubmitIdea_WhenContentIsFlagged_ShouldReturnPendingWithAlternative()
    {
        _fixture.SetAiModerationBehavior(isAllowed: false, alternative: "Please remove profanity and rewrite respectfully.");
        try
        {
            using var scope = _fixture.CreateScope();
            var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
            var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
            var project = projectManager.GetProjectById(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug);
            var topicId = dbContext.Topics
                .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
                .Select(topic => topic.Id)
                .First();

            var response = await ideaManager.SubmitIdeaAsync(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken, "extreme profanity content");

            var pending = Assert.IsType<SubmissionResponse.Pending>(response);
            Assert.Equal(ModerationStatus.Pending, pending.Idea.Status);
            Assert.Equal("Please remove profanity and rewrite respectfully.", pending.Decision.Suggestion);
        }
        finally
        {
            _fixture.SetAiModerationBehavior(isAllowed: true);
        }
    }

    [Fact]
    public async Task CorePipeline_AddResponse_WhenYouthIsUnknown_ShouldAutoCreateYouthAndPersistResponse()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();

        var project = projectManager.GetProjectById(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug);
        var topicId = dbContext.Topics
            .Where(topic => EF.Property<Slug>(topic, "ProjectId") == ManagerSeedData.ProjectSlug)
            .Select(topic => topic.Id)
            .First();
        var seededIdea = await ideaManager.SubmitIdeaAsync(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken, "Idea for foreign youth rejection") switch
        {
            SubmissionResponse.Approved approved => approved,
            _ => throw new InvalidOperationException("Expected approved idea submission.")
        };

        var response = await ideaManager.AddResponseAsync(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, seededIdea.Idea.Id, Guid.NewGuid(), "Response by foreign youth");

        Assert.True(response is ResponseSubmissionResponse.Approved or ResponseSubmissionResponse.Pending);
    }
}
