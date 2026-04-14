using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;
using Conversey.BL.Subplatform;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.BL.Subplatform.Survey.Questions;
using Conversey.DAL;
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
    public void CorePipeline_WorkspaceToProjectQuestionAndAnswerFlow_ShouldPersistAndBeReadableAcrossManagers()
    {
        //Arrange
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
            workspaceId: workspace.Id);

        var youth = projectManager.AddYouth($"pipeline-youth-{Guid.NewGuid():N}", $"pipeline-{Guid.NewGuid():N}@example.com", project.Id);
        _ = projectManager.AddTopic("Pipeline Topic", "Topic for pipeline coverage", project.Id);
        var question = questionManager.AddQuestion(new OpenQuestion
        {
            Text = "What should we improve?",
            IsRequired = true,
            Order = 1,
            Project = project
        });

        var textAnswer = questionManager.AddTextAnswer(new OpenTextAnswer
        {
            Value = "More quiet spaces",
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });

        var integerAnswer = questionManager.AddIntegerAnswer(new IntegerAnswer
        {
            Value = 4,
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });

        // Act
        var workspaceWithProjects = workspaceManager.GetWorkspaceByIdWithProjects(workspace.Id);
        var projectWithQuestions = projectManager.GetProjectByIdWithQuestions(project.Id);
        var loadedTextAnswers = questionManager.GetTextAnswersByQuestionIdWithYouthAndQuestion(question.Id);
        var loadedIntegerAnswers = questionManager.GetIntegerAnswersByQuestionIdWithYouthAndQuestion(question.Id);

        // Assert
        Assert.Contains(workspaceWithProjects.Projects, p => p.Id == project.Id);
        Assert.Contains(projectWithQuestions.Questions, q => q.Id == question.Id);
        Assert.Contains(loadedTextAnswers, answer => answer.Id == textAnswer.Id && answer.YouthToken == youth.Token);
        Assert.Contains(loadedIntegerAnswers, answer => answer.Id == integerAnswer.Id && answer.Value == 4);
    }

    [Fact]
    public void CorePipeline_IdeaToResponseToReactionFlow_ShouldPersistAndBeReadable()
    {
        //Arrange
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
        var topicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;

        var ideaSubmission = Assert.IsType<SubmissionResponse.Approved>(
            ideaManager.SubmitIdea("Pipeline idea content", projectId, topicId, ManagerSeedData.YouthToken));

        var responseSubmission = Assert.IsType<ResponseSubmissionResponse.Approved>(
            ideaManager.AddResponse("Pipeline response content", ideaSubmission.idea.Id, ManagerSeedData.YouthToken));

        var ideaReaction = ideaManager.AddIdeaReaction("like", ideaSubmission.idea.Id, ManagerSeedData.YouthToken);
        var responseReaction = ideaManager.AddResponseReaction("upvote", responseSubmission.Response.Id, ManagerSeedData.YouthToken);

        // Act
        var ideaWithResponses = ideaManager.GetIdeaByIdWithProjectAndResponses(ideaSubmission.idea.Id);
        var ideaReactions = ideaManager.GetIdeaReactionsFromIdeaByIdeaId(ideaSubmission.idea.Id);
        var responseReactions = ideaManager.GetResponseReactionsFromResponseByResponseId(responseSubmission.Response.Id);

        // Assert
        Assert.Equal(IdeaStatus.Approved, ideaWithResponses.Status);
        Assert.Contains(ideaWithResponses.Responses, response => response.Id == responseSubmission.Response.Id);
        Assert.Contains(ideaReactions, reaction => reaction.Id == ideaReaction.Id && reaction.Emoji == "like");
        Assert.Contains(responseReactions, reaction => reaction.Id == responseReaction.Id && reaction.Emoji == "upvote");
    }

    [Fact]
    public void CorePipeline_SubmitIdea_WhenContentIsExtremeProfanity_ShouldReturnPendingWithAlternative()
    {
        //Arrange
        _fixture.SetAiModerationBehavior(isAllowed: false, alternative: "Please remove profanity and rewrite respectfully.");
        try
        {
            using var scope = _fixture.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
            var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
            var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
            var topicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;

            // Act
            var response = ideaManager.SubmitIdea("extreme profanity content", projectId, topicId, ManagerSeedData.YouthToken);

            // Assert
            var pending = Assert.IsType<SubmissionResponse.Pending>(response);
            Assert.Equal(IdeaStatus.Pending, pending.idea.Status);
            Assert.Equal("Please remove profanity and rewrite respectfully.", pending.suggestion);
        }
        finally
        {
            _fixture.SetAiModerationBehavior(isAllowed: true);
        }
    }

    [Fact]
    public void CorePipeline_AddResponse_WhenYouthFromAnotherProject_ShouldThrowValidationExceptionAndNotCreateResponse()
    {
        //Arrange
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();

        var seededProjectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
        var seededTopicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;
        var seededIdea = Assert.IsType<SubmissionResponse.Approved>(
            ideaManager.SubmitIdea("Idea for foreign youth rejection", seededProjectId, seededTopicId, ManagerSeedData.YouthToken));

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
            workspaceId: secondWorkspace.Id);
        var foreignYouth = projectManager.AddYouth($"foreign-youth-{Guid.NewGuid():N}", $"foreign-{Guid.NewGuid():N}@example.com", secondProject.Id);
        var responsesBefore = ideaManager.GetResponsesFromIdeaByIdeaId(seededIdea.idea.Id).Count;

        // Act
        var act = () => ideaManager.AddResponse("Response by foreign youth", seededIdea.idea.Id, foreignYouth.Token);

        // Assert
        Assert.Throws<ValidationException>(act);
        var responsesAfter = ideaManager.GetResponsesFromIdeaByIdeaId(seededIdea.idea.Id).Count;
        Assert.Equal(responsesBefore, responsesAfter);
    }

    [Fact]
    public void CorePipeline_RemoveProject_ShouldMakeProjectQuestionsAndIdeasUnavailable()
    {
        //Arrange
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var workspaceManager = scope.ServiceProvider.GetRequiredService<IWorkspaceManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        var workspaceName = $"Delete Pipeline Workspace {Guid.NewGuid():N}";
        var workspace = workspaceManager.CreateWorkspace(workspaceName, Slug.FromName(workspaceName));
        var project = projectManager.AddProject(
            title: $"Delete Pipeline Project {Guid.NewGuid():N}",
            slug: "delete-pipeline-project",
            description: "Project removal pipeline",
            status: Status.Active,
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddDays(10),
            interactionForm: InteractionType.Chat,
            workspaceId: workspace.Id);

        var topic = projectManager.AddTopic("Delete Topic", "Topic used for delete pipeline", project.Id);
        var youth = projectManager.AddYouth($"delete-youth-{Guid.NewGuid():N}", $"delete-{Guid.NewGuid():N}@example.com", project.Id);
        _ = questionManager.AddQuestion(new OpenQuestion
        {
            Text = "Temporary question",
            IsRequired = false,
            Order = 1,
            Project = project
        });
        _ = ideaManager.SubmitIdea("Temporary idea", project.Id, topic.Id, youth.Token);

        // Act
        projectManager.RemoveProject(project.Id);

        // Assert
        Assert.Throws<ProjectNotFoundException>(() => projectManager.GetProjectById(project.Id));
        Assert.Empty(questionManager.GetQuestionsByProjectId(project.Id));
        Assert.Empty(ideaManager.GetIdeasFromProjectByProjectId(project.Id));
    }
}


