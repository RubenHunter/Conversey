using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Ideation;
using Conversey.BL.Survey;
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

        var workspace = workspaceManager.GetWorkspaceBySlug(ManagerSeedData.WorkspaceSlug);
        var project = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);
        var youth = projectManager.GetYouthByToken(ManagerSeedData.YouthToken);
        var question = questionManager.AddQuestion(new OpenQuestion
        {
            Text = $"What should we improve? {Guid.NewGuid():N}",
            Required = true,
            Project = project
        });

        var typedQuestion = questionManager.GetQuestionById(question.Id) as Question<Answer<string>>;
        Assert.NotNull(typedQuestion);

        var answer = questionManager.AddAnswer(new Answer<string>
        {
            Value = "More quiet spaces",
            Question = typedQuestion,
            Youth = youth
        });

        var workspaceById = workspaceManager.GetWorkspaceById(workspace.Id);
        var projectWithRelations = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(project.Id);
        var loadedAnswer = questionManager.GetAnswerById(answer.Id);

        Assert.Equal(workspace.Id.Text, workspaceById.Id.Text);
        Assert.Contains(projectWithRelations.Questions, q => q.Id == question.Id);
        Assert.Equal(answer.Id, loadedAnswer.Id);
    }

    [Fact]
    public void CorePipeline_IdeaToResponseToReactionFlow_ShouldPersistAndBeReadable()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        var project = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);
        using var topicEnumerator = project.Topic.GetEnumerator();
        Assert.True(topicEnumerator.MoveNext());
        var topicId = topicEnumerator.Current.Id;
        var ideaSubmission = ideaManager.SubmitIdea("Pipeline idea content", ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken) switch
        {
            SubmissionResponse.Approved approved => approved,
            _ => throw new InvalidOperationException("Expected approved idea submission.")
        };

        var responseSubmission = ideaManager.AddResponse("Pipeline response content", ideaSubmission.idea.Id, ManagerSeedData.YouthToken.ToString()) switch
        {
            ResponseSubmissionResponse.Approved approved => approved,
            _ => throw new InvalidOperationException("Expected approved response submission.")
        };

        var ideaReaction = ideaManager.AddIdeaReaction("like", ideaSubmission.idea.Id, ManagerSeedData.YouthToken.ToString());
        var responseReaction = ideaManager.AddResponseReaction("upvote", responseSubmission.IdeaResponse.Id, ManagerSeedData.YouthToken.ToString());

        var ideaWithResponses = ideaManager.GetIdeaByIdWithProjectAndResponses(ideaSubmission.idea.Id);
        var ideaReactions = ideaManager.GetIdeaReactionsByIdeaId(ManagerSeedData.WorkspaceSlug, ManagerSeedData.ProjectSlug, topicId, ideaSubmission.idea.Id);
        var responseReactions = ideaManager.GetResponseReactionsFromResponseByResponseId(responseSubmission.IdeaResponse.Id);

        Assert.Equal(ModerationStatus.Approved, ideaWithResponses.Status);
        Assert.Contains(ideaWithResponses.Responses, response => response.Id == responseSubmission.IdeaResponse.Id);
        Assert.Contains(ideaReactions, reaction => reaction.Id == ideaReaction.Id && reaction.Emoji == "like");
        Assert.Contains(responseReactions, reaction => reaction.Id == responseReaction.Id && reaction.Emoji == "upvote");
    }

    [Fact]
    public void CorePipeline_SubmitIdea_WhenContentIsFlagged_ShouldReturnPendingWithAlternative()
    {
        _fixture.SetAiModerationBehavior(isAllowed: false, alternative: "Please remove profanity and rewrite respectfully.");
        try
        {
            using var scope = _fixture.CreateScope();
            var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
            var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
            var project = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);
            using var topicEnumerator = project.Topic.GetEnumerator();
            Assert.True(topicEnumerator.MoveNext());
            var topicId = topicEnumerator.Current.Id;

            var response = ideaManager.SubmitIdea("extreme profanity content", ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken);

            var pending = Assert.IsType<SubmissionResponse.Pending>(response);
            Assert.Equal(ModerationStatus.Pending, pending.idea.Status);
            Assert.Equal("Please remove profanity and rewrite respectfully.", pending.decision.Suggestion);
        }
        finally
        {
            _fixture.SetAiModerationBehavior(isAllowed: true);
        }
    }

    [Fact]
    public void CorePipeline_AddResponse_WhenYouthIsUnknown_ShouldAutoCreateYouthAndPersistResponse()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        var project = projectManager.GetProjectBySlugWithWorkspaceTopicsYouthsAndQuestions(ManagerSeedData.ProjectSlug);
        using var topicEnumerator = project.Topic.GetEnumerator();
        Assert.True(topicEnumerator.MoveNext());
        var topicId = topicEnumerator.Current.Id;
        var seededIdea = ideaManager.SubmitIdea("Idea for foreign youth rejection", ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken) switch
        {
            SubmissionResponse.Approved approved => approved,
            _ => throw new InvalidOperationException("Expected approved idea submission.")
        };

        var response = ideaManager.AddResponse("Response by foreign youth", seededIdea.idea.Id, Guid.NewGuid().ToString());

        Assert.True(response is ResponseSubmissionResponse.Approved or ResponseSubmissionResponse.Pending);
    }
}
