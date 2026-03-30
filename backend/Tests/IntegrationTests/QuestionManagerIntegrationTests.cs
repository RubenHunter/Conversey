using Conversey.BL.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;
using Conversey.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.IntegrationTests.Infrastructure;

namespace Tests.IntegrationTests;

public class QuestionManagerIntegrationTests : IClassFixture<ManagerIntegrationTestFixture>
{
    private readonly ManagerIntegrationTestFixture _fixture;

    public QuestionManagerIntegrationTests(ManagerIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region QuestionQueries

    [Fact]
    public void GetQuestionById_WhenQuestionExists_ShouldReturnQuestion()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var existingQuestionId = dbContext.Questions.Select(question => question.Id).First();

        // Act
        var question = questionManager.GetQuestionById(existingQuestionId);

        // Assert
        Assert.Equal(existingQuestionId, question.Id);
        Assert.False(string.IsNullOrWhiteSpace(question.Text));
    }

    [Fact]
    public void GetQuestionById_WhenQuestionDoesNotExist_ShouldThrowQuestionNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => questionManager.GetQuestionById(-9999);

        // Assert
        Assert.Throws<QuestionNotFoundException>(act);
    }

    [Fact]
    public void GetQuestionByIdWithProject_WhenQuestionExists_ShouldReturnQuestionWithProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var existingQuestionId = dbContext.Questions.Select(question => question.Id).First();

        // Act
        var question = questionManager.GetQuestionByIdWithProject(existingQuestionId);

        // Assert
        Assert.NotNull(question.Project);
        Assert.True(question.Project.Id > 0);
    }

    [Fact]
    public void GetQuestionsByProjectIdWithProject_ShouldReturnOnlyQuestionsOfThatProject()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var questions = questionManager.GetQuestionsByProjectIdWithProject(projectId);

        // Assert
        Assert.NotEmpty(questions);
        Assert.All(questions, question => Assert.Equal(projectId, question.Project.Id));
    }

    [Fact]
    public void AddQuestion_WhenValidQuestion_ShouldPersistQuestion()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var project = dbContext.Projects.Single(projectEntity => projectEntity.Slug == ManagerSeedData.ProjectSlug);
        var question = new OpenQuestion
        {
            Text = "Integration question",
            IsRequired = true,
            Order = 999,
            Project = project
        };

        // Act
        var createdQuestion = questionManager.AddQuestion(question);

        // Assert
        Assert.True(createdQuestion.Id > 0);
        Assert.Equal(project.Id, createdQuestion.Project.Id);
    }

    [Fact]
    public void RemoveQuestion_WhenQuestionDoesNotExist_ShouldThrowQuestionNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => questionManager.RemoveQuestion(-9999);

        // Assert
        Assert.Throws<QuestionNotFoundException>(act);
    }

    #endregion

    #region AnswerQueriesAndNegativePaths

    [Fact]
    public void AddTextAnswer_WhenValidAnswer_ShouldPersistAndBeRetrievable()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var answer = new OpenTextAnswer
        {
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        };

        // Act
        var createdAnswer = questionManager.AddTextAnswer(answer);
        var loadedAnswer = questionManager.GetTextAnswerByIdWithYouthAndQuestion(createdAnswer.Id);

        // Assert
        Assert.Equal(createdAnswer.Id, loadedAnswer.Id);
        Assert.Equal(youth.Token, loadedAnswer.YouthToken);
        Assert.Equal(question.Id, loadedAnswer.QuestionId);
    }

    [Fact]
    public void GetTextAnswerById_WhenAnswerDoesNotExist_ShouldThrowTextAnswerNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => questionManager.GetTextAnswerById(-9999);

        // Assert
        Assert.Throws<TextAnswerNotFoundException>(act);
    }

    [Fact]
    public void RemoveTextAnswer_WhenAnswerDoesNotExist_ShouldThrowTextAnswerNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => questionManager.RemoveTextAnswer(-9999);

        // Assert
        Assert.Throws<TextAnswerNotFoundException>(act);
    }

    [Fact]
    public void AddIntegerAnswer_WhenValidAnswer_ShouldPersistAndBeRetrievable()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var answer = new IntegerAnswer
        {
            Value = 4,
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        };

        // Act
        var createdAnswer = questionManager.AddIntegerAnswer(answer);
        var loadedAnswer = questionManager.GetIntegerAnswerByIdWithYouthAndQuestion(createdAnswer.Id);

        // Assert
        Assert.Equal(createdAnswer.Id, loadedAnswer.Id);
        Assert.Equal(4, loadedAnswer.Value);
        Assert.Equal(youth.Token, loadedAnswer.YouthToken);
    }

    [Fact]
    public void GetIntegerAnswerById_WhenAnswerDoesNotExist_ShouldThrowIntegerAnswerNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => questionManager.GetIntegerAnswerById(-9999);

        // Assert
        Assert.Throws<IntegerAnswerNotFoundException>(act);
    }

    [Fact]
    public void RemoveIntegerAnswer_WhenAnswerDoesNotExist_ShouldThrowIntegerAnswerNotFoundException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => questionManager.RemoveIntegerAnswer(-9999);

        // Assert
        Assert.Throws<IntegerAnswerNotFoundException>(act);
    }

    #endregion
}


