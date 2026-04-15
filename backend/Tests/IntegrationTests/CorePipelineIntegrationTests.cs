using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Ideation;
using Conversey.BL.Subplatform.Survey.Questions;
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
    public void CorePipeline_WorkspaceToProjectQuestionAndAnswerFlow_ShouldPersistAcrossManagers()
    {
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        var workspaceName = $"Pipeline Workspace {Guid.NewGuid():N}";
        var workspace = workspaceManager.CreateWorkspace(workspaceName, Slug.FromName(workspaceName));
        var project = projectManager.AddProject(
            title: $"Pipeline Project {Guid.NewGuid():N}",
            slug: "pipeline-project",
            description: "Core pipeline integration test",
            status: Status.Active,
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddDays(14),
            interactionForm: InteractionType.Chat,
            workspaceSlug: workspace.Id);

        var youth = projectManager.AddYouth(Guid.NewGuid(), $"pipeline-{Guid.NewGuid():N}@example.com", project.Id);
        _ = projectManager.AddTopic("Pipeline Topic", "Topic for pipeline coverage", project.Id);
        var question = questionManager.AddQuestion(new OpenQuestion
        {
            Text = "What should we improve?",
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

        var workspaceWithProjects = workspaceManager.GetWorkspaceByIdWithProjects(workspace.Id);
        var projectWithQuestions = projectManager.GetProjectBySlugWithQuestions(project.Id);
        var loadedAnswer = questionManager.GetAnswerById(answer.Id);

        Assert.Contains(workspaceWithProjects.Projects, p => p.Id == project.Id);
        Assert.Contains(projectWithQuestions.Questions, q => q.Id == question.Id);
        Assert.Equal(answer.Id, loadedAnswer.Id);
    }

    [Fact]
    public void CorePipeline_IdeaToResponseToReactionFlow_ShouldPersistAndBeReadable()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        var topicId = projectManager.GetTopicsFromProjectByProjectId(ManagerSeedData.ProjectSlug).First().Id;
        var ideaSubmission = Assert.IsType<SubmissionResponse.Approved>(
            ideaManager.SubmitIdea("Pipeline idea content", ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken));

        var responseSubmission = Assert.IsType<ResponseSubmissionResponse.Approved>(
            ideaManager.AddResponse("Pipeline response content", ideaSubmission.idea.Id, ManagerSeedData.YouthToken.ToString()));

        var ideaReaction = ideaManager.AddIdeaReaction("like", ideaSubmission.idea.Id, ManagerSeedData.YouthToken.ToString());
        var responseReaction = ideaManager.AddResponseReaction("upvote", responseSubmission.Response.Id, ManagerSeedData.YouthToken.ToString());

        var ideaWithResponses = ideaManager.GetIdeaByIdWithProjectAndResponses(ideaSubmission.idea.Id);
        var ideaReactions = ideaManager.GetIdeaReactionsFromIdeaByIdeaId(ideaSubmission.idea.Id);
        var responseReactions = ideaManager.GetResponseReactionsFromResponseByResponseId(responseSubmission.Response.Id);

        Assert.Equal(ModerationStatus.Approved, ideaWithResponses.Status);
        Assert.Contains(ideaWithResponses.Responses, response => response.Id == responseSubmission.Response.Id);
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
            var topicId = projectManager.GetTopicsFromProjectByProjectId(ManagerSeedData.ProjectSlug).First().Id;

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
    public void CorePipeline_AddResponse_WhenYouthFromAnotherProject_ShouldThrowValidationException()
    {
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        var topicId = projectManager.GetTopicsFromProjectByProjectId(ManagerSeedData.ProjectSlug).First().Id;
        var seededIdea = Assert.IsType<SubmissionResponse.Approved>(
            ideaManager.SubmitIdea("Idea for foreign youth rejection", ManagerSeedData.ProjectSlug, topicId, ManagerSeedData.YouthToken));

        var secondWorkspaceName = $"Foreign Workspace {Guid.NewGuid():N}";
        var secondWorkspace = workspaceManager.CreateWorkspace(secondWorkspaceName, Slug.FromName(secondWorkspaceName));
        var secondProject = projectManager.AddProject(
            title: $"Foreign Project {Guid.NewGuid():N}",
            slug: "foreign-project",
            description: "Foreign youth scope",
            status: Status.Active,
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddDays(7),
            interactionForm: InteractionType.Chat,
            workspaceSlug: secondWorkspace.Id);
        var foreignYouth = projectManager.AddYouth(Guid.NewGuid(), $"foreign-{Guid.NewGuid():N}@example.com", secondProject.Id);

        var act = () => ideaManager.AddResponse("Response by foreign youth", seededIdea.idea.Id, foreignYouth.Id.ToString());

        Assert.Throws<ValidationException>(act);
    }
}
