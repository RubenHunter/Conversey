using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.BL.Ideation;
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

    public static IEnumerable<object[]> IdeaByIdReaders()
    {
        yield return new object[] { "GetIdeaById", (Func<IIdeaManager, int, Idea>)((manager, id) => manager.GetIdeaById(id)) };
        yield return new object[] { "GetIdeaByIdWithProject", (Func<IIdeaManager, int, Idea>)((manager, id) => manager.GetIdeaByIdWithProject(id)) };
        yield return new object[] { "GetIdeaByIdWithResponses", (Func<IIdeaManager, int, Idea>)((manager, id) => manager.GetIdeaByIdWithResponses(id)) };
        yield return new object[] { "GetIdeaByIdWithProjectAndResponses", (Func<IIdeaManager, int, Idea>)((manager, id) => manager.GetIdeaByIdWithProjectAndResponses(id)) };
    }

    public static IEnumerable<object[]> IdeaCollectionReaders()
    {
        yield return new object[] { "GetAllIdeas", (Func<IIdeaManager, IReadOnlyCollection<Idea>>)(manager => manager.GetAllIdeas()) };
        yield return new object[] { "GetAllIdeasWithProject", (Func<IIdeaManager, IReadOnlyCollection<Idea>>)(manager => manager.GetAllIdeasWithProject()) };
        yield return new object[] { "GetAllIdeasWithResponses", (Func<IIdeaManager, IReadOnlyCollection<Idea>>)(manager => manager.GetAllIdeasWithResponses()) };
        yield return new object[] { "GetAllIdeasWithProjectAndResponses", (Func<IIdeaManager, IReadOnlyCollection<Idea>>)(manager => manager.GetAllIdeasWithProjectAndResponses()) };
    }

    public static IEnumerable<object[]> ResponseByIdReaders()
    {
        yield return new object[] { "GetResponseById", (Func<IIdeaManager, int, Response>)((manager, id) => manager.GetResponseById(id)) };
        yield return new object[] { "GetResponseByIdWithIdea", (Func<IIdeaManager, int, Response>)((manager, id) => manager.GetResponseByIdWithIdea(id)) };
    }

    public static IEnumerable<object[]> ResponseCollectionReaders()
    {
        yield return new object[] { "GetResponsesFromIdeaByIdeaId", (Func<IIdeaManager, int, IReadOnlyCollection<Response>>)((manager, ideaId) => manager.GetResponsesFromIdeaByIdeaId(ideaId)) };
        yield return new object[] { "GetResponsesFromIdeaByIdeaIdWithIdea", (Func<IIdeaManager, int, IReadOnlyCollection<Response>>)((manager, ideaId) => manager.GetResponsesFromIdeaByIdeaIdWithIdea(ideaId)) };
    }

    #region IdeaQueries

    [Theory]
    [MemberData(nameof(IdeaByIdReaders))]
    public void IdeaReaders_WhenIdeaExists_ShouldReturnIdea(string _, Func<IIdeaManager, int, Idea> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();

        // Act
        var idea = readMethod(ideaManager, ideaId);

        // Assert
        Assert.Equal(ideaId, idea.Id);
        Assert.False(string.IsNullOrWhiteSpace(idea.Content));
    }

    [Theory]
    [MemberData(nameof(IdeaByIdReaders))]
    public void IdeaReaders_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException(string _, Func<IIdeaManager, int, Idea> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => readMethod(ideaManager, -9999);

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

    [Theory]
    [MemberData(nameof(IdeaCollectionReaders))]
    public void IdeaCollectionReaders_WhenSeededDataExists_ShouldReturnIdeas(string _, Func<IIdeaManager, IReadOnlyCollection<Idea>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var ideas = readMethod(ideaManager);

        // Assert
        Assert.NotEmpty(ideas);
    }

    [Fact]
    public void GetIdeasFromProjectByProjectId_WhenProjectDoesNotExist_ShouldReturnEmptyCollection()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var ideas = ideaManager.GetIdeasFromProjectByProjectId(-9999);

        // Assert
        Assert.Empty(ideas);
    }

    [Fact]
    public void GetIdeasFromProjectByProjectIdWithResponses_WhenProjectDoesNotExist_ShouldReturnEmptyCollection()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var ideas = ideaManager.GetIdeasFromProjectByProjectIdWithResponses(-9999);

        // Assert
        Assert.Empty(ideas);
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

    [Fact]
    public void GetIdeasFromTopicByProjectSlugAndTopicId_WhenTopicDoesNotExist_ShouldReturnEmptyCollection()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var ideas = ideaManager.GetIdeasFromTopicByProjectSlugAndTopicId(ManagerSeedData.ProjectSlug, -9999);

        // Assert
        Assert.Empty(ideas);
    }

    #endregion

    #region SubmitIdea

    [Fact]
    public void SubmitIdea_WhenPayloadIsValid_ShouldReturnApprovedSubmission()
    {
        //Arrange
        _fixture.SetAiModerationBehavior(isAllowed: true);
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
    public void SubmitIdea_WhenModerationFlagsContent_ShouldReturnPendingWithSuggestion()
    {
        //Arrange
        _fixture.SetAiModerationBehavior(isAllowed: false, alternative: "Please avoid profanity and rewrite this respectfully.");
        try
        {
            using var scope = _fixture.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
            var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
            var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
            var topicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;

            // Act
            var response = ideaManager.SubmitIdea("extreme profanity payload", projectId, topicId, ManagerSeedData.YouthToken);

            // Assert
            var pending = Assert.IsType<SubmissionResponse.Pending>(response);
            Assert.Equal(IdeaStatus.Pending, pending.idea.Status);
            Assert.Equal("Please avoid profanity and rewrite this respectfully.", pending.suggestion);
        }
        finally
        {
            _fixture.SetAiModerationBehavior(isAllowed: true);
        }
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

    [Fact]
    public void ChangeIdea_WhenIdeaIsValid_ShouldPersistChanges()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = ideaManager.GetAllIdeas().First().Id;
        var idea = ideaManager.GetIdeaByIdWithProject(ideaId);
        idea.Summary = "Updated summary";

        // Act
        var changedIdea = ideaManager.ChangeIdea(idea);

        // Assert
        Assert.Equal("Updated summary", changedIdea.Summary);
    }

    [Fact]
    public void ChangeIdea_WhenIdeaIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = ideaManager.GetAllIdeas().First().Id;
        var idea = ideaManager.GetIdeaByIdWithProject(ideaId);
        idea.Content = new string('x', 4500);

        // Act
        var act = () => ideaManager.ChangeIdea(idea);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void RemoveIdea_WhenIdeaExists_ShouldDeleteIdea()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;
        var topicId = dbContext.Topics.First(topic => topic.Project.Slug == ManagerSeedData.ProjectSlug).Id;
        var submission = Assert.IsType<SubmissionResponse.Approved>(ideaManager.SubmitIdea("Temporary delete idea", projectId, topicId, ManagerSeedData.YouthToken));

        // Act
        ideaManager.RemoveIdea(submission.idea.Id);

        // Assert
        Assert.Throws<IdeaNotFoundException>(() => ideaManager.GetIdeaById(submission.idea.Id));
    }

    [Fact]
    public void RemoveIdea_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.RemoveIdea(-9999);

        // Assert
        Assert.Throws<IdeaNotFoundException>(act);
    }

    #endregion

    #region Responses

    [Fact]
    public void AddResponse_WhenPayloadIsValid_ShouldReturnApprovedResponse()
    {
        //Arrange
        _fixture.SetAiModerationBehavior(isAllowed: true);
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();

        // Act
        var response = ideaManager.AddResponse("Integration response", ideaId, ManagerSeedData.YouthToken);

        // Assert
        var approved = Assert.IsType<ResponseSubmissionResponse.Approved>(response);
        Assert.Equal(IdeaStatus.Approved, approved.Response.Status);
        Assert.Equal(ManagerSeedData.YouthToken, approved.Response.Youth.Token);
    }

    [Fact]
    public void AddResponse_WhenModerationFlagsContent_ShouldReturnPendingWithSuggestion()
    {
        //Arrange
        _fixture.SetAiModerationBehavior(isAllowed: false, alternative: "Please avoid abusive language in your response.");
        try
        {
            using var scope = _fixture.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
            var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
            var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();

            // Act
            var response = ideaManager.AddResponse("extreme profanity response", ideaId, ManagerSeedData.YouthToken);

            // Assert
            var pending = Assert.IsType<ResponseSubmissionResponse.Pending>(response);
            Assert.Equal(IdeaStatus.Pending, pending.Response.Status);
            Assert.Equal("Please avoid abusive language in your response.", pending.Suggestion);
        }
        finally
        {
            _fixture.SetAiModerationBehavior(isAllowed: true);
        }
    }

    [Fact]
    public void AddResponse_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.AddResponse("text", -9999, ManagerSeedData.YouthToken);

        // Assert
        Assert.Throws<IdeaNotFoundException>(act);
    }

    [Fact]
    public void AddResponse_WhenYouthDoesNotBelongToProject_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();

        // Act
        var act = () => ideaManager.AddResponse("text", ideaId, "unknown-youth-token");

        // Assert
        Assert.Throws<YouthNotFoundException>(act);
    }

    [Theory]
    [MemberData(nameof(ResponseByIdReaders))]
    public void ResponseReaders_WhenResponseExists_ShouldReturnResponse(string _, Func<IIdeaManager, int, Response> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var responseId = dbContext.Responses.Select(response => response.Id).First();

        // Act
        var response = readMethod(ideaManager, responseId);

        // Assert
        Assert.Equal(responseId, response.Id);
        Assert.False(string.IsNullOrWhiteSpace(response.Text));
    }

    [Theory]
    [MemberData(nameof(ResponseByIdReaders))]
    public void ResponseReaders_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException(string _, Func<IIdeaManager, int, Response> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => readMethod(ideaManager, -9999);

        // Assert
        Assert.Throws<ResponseNotFoundException>(act);
    }

    [Theory]
    [MemberData(nameof(ResponseCollectionReaders))]
    public void ResponseCollectionReaders_WhenIdeaExists_ShouldReturnResponses(string _, Func<IIdeaManager, int, IReadOnlyCollection<Response>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Responses.Select(response => response.Idea.Id).First();

        // Act
        var responses = readMethod(ideaManager, ideaId);

        // Assert
        Assert.NotEmpty(responses);
    }

    [Theory]
    [MemberData(nameof(ResponseCollectionReaders))]
    public void ResponseCollectionReaders_WhenIdeaDoesNotExist_ShouldReturnEmpty(string _, Func<IIdeaManager, int, IReadOnlyCollection<Response>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var responses = readMethod(ideaManager, -9999);

        // Assert
        Assert.Empty(responses);
    }

    [Fact]
    public void ChangeResponse_WhenResponseIsValid_ShouldPersistChanges()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var response = ideaManager.GetResponseById(ideaManager.GetResponsesFromIdeaByIdeaId(ideaManager.GetAllIdeas().First().Id).First().Id);
        response.Text = "Updated response";

        // Act
        var changedResponse = ideaManager.ChangeResponse(response);

        // Assert
        Assert.Equal("Updated response", changedResponse.Text);
    }

    [Fact]
    public void ChangeResponse_WhenResponseIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var response = ideaManager.GetResponseByIdWithIdea(ideaManager.GetResponsesFromIdeaByIdeaId(ideaManager.GetAllIdeas().First().Id).First().Id);
        response.Text = string.Empty;

        // Act
        var act = () => ideaManager.ChangeResponse(response);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void RemoveResponse_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.RemoveResponse(-9999);

        // Assert
        Assert.Throws<ResponseNotFoundException>(act);
    }

    [Fact]
    public void RemoveResponse_WhenResponseExists_ShouldDeleteResponse()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();
        var submission = Assert.IsType<ResponseSubmissionResponse.Approved>(ideaManager.AddResponse("Temporary response", ideaId, ManagerSeedData.YouthToken));

        // Act
        ideaManager.RemoveResponse(submission.Response.Id);

        // Assert
        Assert.Throws<ResponseNotFoundException>(() => ideaManager.GetResponseById(submission.Response.Id));
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
    public void AddIdeaReaction_WhenIdeaDoesNotExist_ShouldThrowIdeaNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.AddIdeaReaction("👍", -9999, ManagerSeedData.YouthToken);

        // Assert
        Assert.Throws<IdeaNotFoundException>(act);
    }

    [Fact]
    public void AddIdeaReaction_WhenYouthDoesNotExist_ShouldThrowYouthNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();

        // Act
        var act = () => ideaManager.AddIdeaReaction("👍", ideaId, "unknown-youth-token");

        // Assert
        Assert.Throws<YouthNotFoundException>(act);
    }

    [Fact]
    public void GetIdeaReactionsFromIdeaByIdeaId_WhenIdeaExists_ShouldReturnReactions()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();
        _ = ideaManager.AddIdeaReaction("👍", ideaId, ManagerSeedData.YouthToken);

        // Act
        var reactions = ideaManager.GetIdeaReactionsFromIdeaByIdeaId(ideaId);

        // Assert
        Assert.NotEmpty(reactions);
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
    public void RemoveIdeaReaction_WhenReactionExists_ShouldDeleteReaction()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var ideaId = dbContext.Ideas.Select(idea => idea.Id).First();
        _ = ideaManager.AddIdeaReaction("🔥", ideaId, ManagerSeedData.YouthToken);

        // Act
        ideaManager.RemoveIdeaReaction(ideaId, ManagerSeedData.YouthToken, "🔥");

        // Assert
        var reactions = ideaManager.GetIdeaReactionsFromIdeaByIdeaId(ideaId);
        Assert.DoesNotContain(reactions, reaction => reaction.YouthToken == ManagerSeedData.YouthToken && reaction.Emoji == "🔥");
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
    public void AddResponseReaction_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.AddResponseReaction("👍", -9999, ManagerSeedData.YouthToken);

        // Assert
        Assert.Throws<ResponseNotFoundException>(act);
    }

    [Fact]
    public void AddResponseReaction_WhenResponseExists_ShouldCreateReaction()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var responseId = dbContext.Responses.Select(response => response.Id).First();

        // Act
        var reaction = ideaManager.AddResponseReaction("👍", responseId, ManagerSeedData.YouthToken);

        // Assert
        Assert.True(reaction.Id > 0);
        Assert.Equal("👍", reaction.Emoji);
    }

    [Fact]
    public void GetResponseReactionsFromResponseByResponseId_WhenResponseDoesNotExist_ShouldThrowResponseNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();

        // Act
        var act = () => ideaManager.GetResponseReactionsFromResponseByResponseId(-9999);

        // Assert
        Assert.Throws<ResponseNotFoundException>(act);
    }

    [Fact]
    public void GetResponseReactionsFromResponseByResponseId_WhenResponseExists_ShouldReturnReactions()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var responseId = dbContext.Responses.Select(response => response.Id).First();
        _ = ideaManager.AddResponseReaction("💡", responseId, ManagerSeedData.YouthToken);

        // Act
        var reactions = ideaManager.GetResponseReactionsFromResponseByResponseId(responseId);

        // Assert
        Assert.NotEmpty(reactions);
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

    [Fact]
    public void RemoveResponseReaction_WhenReactionExists_ShouldDeleteReaction()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var ideaManager = scope.ServiceProvider.GetRequiredService<IIdeaManager>();
        var responseId = dbContext.Responses.Select(response => response.Id).First();
        _ = ideaManager.AddResponseReaction("🎯", responseId, ManagerSeedData.YouthToken);

        // Act
        ideaManager.RemoveResponseReaction(responseId, ManagerSeedData.YouthToken, "🎯");

        // Assert
        var reactions = ideaManager.GetResponseReactionsFromResponseByResponseId(responseId);
        Assert.DoesNotContain(reactions, reaction => reaction.YouthToken == ManagerSeedData.YouthToken && reaction.Emoji == "🎯");
    }

    #endregion
}





