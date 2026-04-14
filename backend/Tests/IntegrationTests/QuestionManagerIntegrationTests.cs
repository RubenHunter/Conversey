using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;
using Conversey.BL.Survey;
using Conversey.DAL;
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

    public static IEnumerable<object[]> QuestionByIdReaders()
    {
        yield return new object[] { "GetQuestionById", (Func<IQuestionManager, int, Question>)((manager, id) => manager.GetQuestionById(id)) };
        yield return new object[] { "GetQuestionByIdWithProject", (Func<IQuestionManager, int, Question>)((manager, id) => manager.GetQuestionByIdWithProject(id)) };
    }

    public static IEnumerable<object[]> QuestionCollectionReaders()
    {
        yield return new object[] { "GetAllQuestions", (Func<IQuestionManager, IReadOnlyCollection<Question>>)(manager => manager.GetAllQuestions()) };
        yield return new object[] { "GetAllQuestionsWithProject", (Func<IQuestionManager, IReadOnlyCollection<Question>>)(manager => manager.GetAllQuestionsWithProject()) };
    }

    public static IEnumerable<object[]> ProjectQuestionReaders()
    {
        yield return new object[] { "GetQuestionsByProjectId", (Func<IQuestionManager, int, IReadOnlyCollection<Question>>)((manager, projectId) => manager.GetQuestionsByProjectId(projectId)) };
        yield return new object[] { "GetQuestionsByProjectIdWithProject", (Func<IQuestionManager, int, IReadOnlyCollection<Question>>)((manager, projectId) => manager.GetQuestionsByProjectIdWithProject(projectId)) };
    }

    public static IEnumerable<object[]> TextAnswerByIdReaders()
    {
        yield return new object[] { "GetTextAnswerById", (Func<IQuestionManager, int, TextAnswer>)((manager, id) => manager.GetTextAnswerById(id)) };
        yield return new object[] { "GetTextAnswerByIdWithYouth", (Func<IQuestionManager, int, TextAnswer>)((manager, id) => manager.GetTextAnswerByIdWithYouth(id)) };
        yield return new object[] { "GetTextAnswerByIdWithQuestion", (Func<IQuestionManager, int, TextAnswer>)((manager, id) => manager.GetTextAnswerByIdWithQuestion(id)) };
        yield return new object[] { "GetTextAnswerByIdWithYouthAndQuestion", (Func<IQuestionManager, int, TextAnswer>)((manager, id) => manager.GetTextAnswerByIdWithYouthAndQuestion(id)) };
    }

    public static IEnumerable<object[]> TextAnswerCollectionReaders()
    {
        yield return new object[] { "GetTextAnswersByQuestionId", (Func<IQuestionManager, int, IReadOnlyCollection<TextAnswer>>)((manager, id) => manager.GetTextAnswersByQuestionId(id)) };
        yield return new object[] { "GetTextAnswersByQuestionIdWithYouth", (Func<IQuestionManager, int, IReadOnlyCollection<TextAnswer>>)((manager, id) => manager.GetTextAnswersByQuestionIdWithYouth(id)) };
        yield return new object[] { "GetTextAnswersByQuestionIdWithQuestion", (Func<IQuestionManager, int, IReadOnlyCollection<TextAnswer>>)((manager, id) => manager.GetTextAnswersByQuestionIdWithQuestion(id)) };
        yield return new object[] { "GetTextAnswersByQuestionIdWithYouthAndQuestion", (Func<IQuestionManager, int, IReadOnlyCollection<TextAnswer>>)((manager, id) => manager.GetTextAnswersByQuestionIdWithYouthAndQuestion(id)) };
    }

    public static IEnumerable<object[]> IntegerAnswerByIdReaders()
    {
        yield return new object[] { "GetIntegerAnswerById", (Func<IQuestionManager, int, IntegerAnswer>)((manager, id) => manager.GetIntegerAnswerById(id)) };
        yield return new object[] { "GetIntegerAnswerByIdWithYouth", (Func<IQuestionManager, int, IntegerAnswer>)((manager, id) => manager.GetIntegerAnswerByIdWithYouth(id)) };
        yield return new object[] { "GetIntegerAnswerByIdWithQuestion", (Func<IQuestionManager, int, IntegerAnswer>)((manager, id) => manager.GetIntegerAnswerByIdWithQuestion(id)) };
        yield return new object[] { "GetIntegerAnswerByIdWithYouthAndQuestion", (Func<IQuestionManager, int, IntegerAnswer>)((manager, id) => manager.GetIntegerAnswerByIdWithYouthAndQuestion(id)) };
    }

    public static IEnumerable<object[]> IntegerAnswerCollectionReaders()
    {
        yield return new object[] { "GetIntegerAnswersByQuestionId", (Func<IQuestionManager, int, IReadOnlyCollection<IntegerAnswer>>)((manager, id) => manager.GetIntegerAnswersByQuestionId(id)) };
        yield return new object[] { "GetIntegerAnswersByQuestionIdWithYouth", (Func<IQuestionManager, int, IReadOnlyCollection<IntegerAnswer>>)((manager, id) => manager.GetIntegerAnswersByQuestionIdWithYouth(id)) };
        yield return new object[] { "GetIntegerAnswersByQuestionIdWithQuestion", (Func<IQuestionManager, int, IReadOnlyCollection<IntegerAnswer>>)((manager, id) => manager.GetIntegerAnswersByQuestionIdWithQuestion(id)) };
        yield return new object[] { "GetIntegerAnswersByQuestionIdWithYouthAndQuestion", (Func<IQuestionManager, int, IReadOnlyCollection<IntegerAnswer>>)((manager, id) => manager.GetIntegerAnswersByQuestionIdWithYouthAndQuestion(id)) };
    }

    #region QuestionQueries

    [Theory]
    [MemberData(nameof(QuestionByIdReaders))]
    public void QuestionByIdReaders_WhenQuestionExists_ShouldReturnQuestion(string _, Func<IQuestionManager, int, Question> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var existingQuestionId = dbContext.Questions.Select(question => question.Id).First();

        // Act
        var question = readMethod(questionManager, existingQuestionId);

        // Assert
        Assert.Equal(existingQuestionId, question.Id);
        Assert.False(string.IsNullOrWhiteSpace(question.Text));
    }

    [Theory]
    [MemberData(nameof(QuestionByIdReaders))]
    public void QuestionByIdReaders_WhenQuestionDoesNotExist_ShouldThrowQuestionNotFoundException(string _, Func<IQuestionManager, int, Question> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => readMethod(questionManager, -9999);

        // Assert
        Assert.Throws<QuestionNotFoundException>(act);
    }

    [Theory]
    [MemberData(nameof(QuestionCollectionReaders))]
    public void QuestionCollectionReaders_WhenSeededDataExists_ShouldReturnQuestions(string _, Func<IQuestionManager, IReadOnlyCollection<Question>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var questions = readMethod(questionManager);

        // Assert
        Assert.NotEmpty(questions);
    }

    [Theory]
    [MemberData(nameof(ProjectQuestionReaders))]
    public void ProjectQuestionReaders_WhenProjectExists_ShouldReturnQuestions(string _, Func<IQuestionManager, int, IReadOnlyCollection<Question>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var projectId = dbContext.Projects.Single(project => project.Slug == ManagerSeedData.ProjectSlug).Id;

        // Act
        var questions = readMethod(questionManager, projectId);

        // Assert
        Assert.NotEmpty(questions);
    }

    [Theory]
    [MemberData(nameof(ProjectQuestionReaders))]
    public void ProjectQuestionReaders_WhenProjectDoesNotExist_ShouldReturnEmpty(string _, Func<IQuestionManager, int, IReadOnlyCollection<Question>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var questions = readMethod(questionManager, -9999);

        // Assert
        Assert.Empty(questions);
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
    public void AddQuestion_WhenQuestionIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var project = dbContext.Projects.Single(projectEntity => projectEntity.Slug == ManagerSeedData.ProjectSlug);
        var invalidQuestion = new SingleChoiceQuestion
        {
            Text = "Invalid single choice",
            IsRequired = true,
            Order = 1000,
            Project = project,
            Options = new List<QuestionOption>
            {
                new() { Text = "Only one option", Order = 1 }
            }
        };

        // Act
        var act = () => questionManager.AddQuestion(invalidQuestion);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void ChangeQuestion_WhenQuestionIsValid_ShouldPersistChanges()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = questionManager.GetAllQuestions().First();
        question.Text = "Updated question text";

        // Act
        var changedQuestion = questionManager.ChangeQuestion(question);

        // Assert
        Assert.Equal("Updated question text", changedQuestion.Text);
    }

    [Fact]
    public void ChangeQuestion_WhenQuestionIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = questionManager.GetAllQuestions().First();
        question.Text = new string('q', 700);

        // Act
        var act = () => questionManager.ChangeQuestion(question);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void RemoveQuestion_WhenQuestionExists_ShouldDeleteQuestion()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var project = dbContext.Projects.Single(projectEntity => projectEntity.Slug == ManagerSeedData.ProjectSlug);
        var question = questionManager.AddQuestion(new OpenQuestion
        {
            Text = "Temporary removable question",
            IsRequired = false,
            Order = 1234,
            Project = project
        });

        // Act
        questionManager.RemoveQuestion(question.Id);

        // Assert
        Assert.Throws<QuestionNotFoundException>(() => questionManager.GetQuestionById(question.Id));
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

    [Theory]
    [MemberData(nameof(TextAnswerByIdReaders))]
    public void TextAnswerByIdReaders_WhenAnswerExists_ShouldReturnAnswer(string _, Func<IQuestionManager, int, TextAnswer> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var created = questionManager.AddTextAnswer(new OpenTextAnswer
        {
            Value = "Answer",
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });

        // Act
        var loaded = readMethod(questionManager, created.Id);

        // Assert
        Assert.Equal(created.Id, loaded.Id);
    }

    [Theory]
    [MemberData(nameof(TextAnswerByIdReaders))]
    public void TextAnswerByIdReaders_WhenAnswerDoesNotExist_ShouldThrowTextAnswerNotFoundException(string _, Func<IQuestionManager, int, TextAnswer> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => readMethod(questionManager, -9999);

        // Assert
        Assert.Throws<TextAnswerNotFoundException>(act);
    }

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

    [Theory]
    [MemberData(nameof(TextAnswerCollectionReaders))]
    public void TextAnswerCollectionReaders_WhenQuestionExists_ShouldReturnAnswers(string _, Func<IQuestionManager, int, IReadOnlyCollection<TextAnswer>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var createdAnswer = questionManager.AddTextAnswer(new OpenTextAnswer
        {
            Value = "Collection answer",
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });

        // Act
        var answers = readMethod(questionManager, question.Id);

        // Assert
        Assert.True(createdAnswer.Id > 0);
        Assert.NotEmpty(answers);
    }

    [Theory]
    [MemberData(nameof(TextAnswerCollectionReaders))]
    public void TextAnswerCollectionReaders_WhenQuestionDoesNotExist_ShouldReturnEmpty(string _, Func<IQuestionManager, int, IReadOnlyCollection<TextAnswer>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var answers = readMethod(questionManager, -9999);

        // Assert
        Assert.Empty(answers);
    }

    [Fact]
    public void ChangeTextAnswer_WhenAnswerIsValid_ShouldPersistChanges()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var answer = (OpenTextAnswer)questionManager.AddTextAnswer(new OpenTextAnswer
        {
            Value = "Before",
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });
        answer.Value = "After";

        // Act
        var changed = questionManager.ChangeTextAnswer(answer);

        // Assert
        Assert.Equal("After", ((OpenTextAnswer)changed).Value);
    }

    [Fact]
    public void ChangeTextAnswer_WhenAnswerIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var invalidAnswer = new OpenTextAnswer
        {
            Value = "Invalid",
            YouthToken = string.Empty
        };

        // Act
        var act = () => questionManager.ChangeTextAnswer(invalidAnswer);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void RemoveTextAnswer_WhenAnswerExists_ShouldDeleteAnswer()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var answer = questionManager.AddTextAnswer(new OpenTextAnswer
        {
            Value = "Removable",
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });

        // Act
        questionManager.RemoveTextAnswer(answer.Id);

        // Assert
        Assert.Throws<TextAnswerNotFoundException>(() => questionManager.GetTextAnswerById(answer.Id));
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

    [Theory]
    [MemberData(nameof(IntegerAnswerByIdReaders))]
    public void IntegerAnswerByIdReaders_WhenAnswerExists_ShouldReturnAnswer(string _, Func<IQuestionManager, int, IntegerAnswer> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var created = questionManager.AddIntegerAnswer(new IntegerAnswer
        {
            Value = 5,
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });

        // Act
        var loaded = readMethod(questionManager, created.Id);

        // Assert
        Assert.Equal(created.Id, loaded.Id);
    }

    [Theory]
    [MemberData(nameof(IntegerAnswerByIdReaders))]
    public void IntegerAnswerByIdReaders_WhenAnswerDoesNotExist_ShouldThrowIntegerAnswerNotFoundException(string _, Func<IQuestionManager, int, IntegerAnswer> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var act = () => readMethod(questionManager, -9999);

        // Assert
        Assert.Throws<IntegerAnswerNotFoundException>(act);
    }

    [Theory]
    [MemberData(nameof(IntegerAnswerCollectionReaders))]
    public void IntegerAnswerCollectionReaders_WhenQuestionExists_ShouldReturnAnswers(string _, Func<IQuestionManager, int, IReadOnlyCollection<IntegerAnswer>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var createdAnswer = questionManager.AddIntegerAnswer(new IntegerAnswer
        {
            Value = 3,
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });

        // Act
        var answers = readMethod(questionManager, question.Id);

        // Assert
        Assert.True(createdAnswer.Id > 0);
        Assert.NotEmpty(answers);
    }

    [Theory]
    [MemberData(nameof(IntegerAnswerCollectionReaders))]
    public void IntegerAnswerCollectionReaders_WhenQuestionDoesNotExist_ShouldReturnEmpty(string _, Func<IQuestionManager, int, IReadOnlyCollection<IntegerAnswer>> readMethod)
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        // Act
        var answers = readMethod(questionManager, -9999);

        // Assert
        Assert.Empty(answers);
    }

    [Fact]
    public void AddIntegerAnswer_WhenAnswerIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var invalidAnswer = new IntegerAnswer
        {
            Value = 2,
            YouthToken = string.Empty
        };

        // Act
        var act = () => questionManager.AddIntegerAnswer(invalidAnswer);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void ChangeIntegerAnswer_WhenAnswerIsValid_ShouldPersistChanges()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var answer = questionManager.AddIntegerAnswer(new IntegerAnswer
        {
            Value = 2,
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });
        answer.Value = 9;

        // Act
        var changed = questionManager.ChangeIntegerAnswer(answer);

        // Assert
        Assert.Equal(9, changed.Value);
    }

    [Fact]
    public void ChangeIntegerAnswer_WhenAnswerIsInvalid_ShouldThrowValidationException()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var invalidAnswer = new IntegerAnswer
        {
            Value = 1,
            YouthToken = string.Empty
        };

        // Act
        var act = () => questionManager.ChangeIntegerAnswer(invalidAnswer);

        // Assert
        Assert.Throws<ValidationException>(act);
    }

    [Fact]
    public void RemoveIntegerAnswer_WhenAnswerExists_ShouldDeleteAnswer()
    {
        //Arrange
        using var scope = _fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var question = dbContext.Questions.First();
        var youth = dbContext.Youths.First(y => y.Token == ManagerSeedData.YouthToken);
        var answer = questionManager.AddIntegerAnswer(new IntegerAnswer
        {
            Value = 7,
            YouthToken = youth.Token,
            Youth = youth,
            QuestionId = question.Id,
            Question = question
        });

        // Act
        questionManager.RemoveIntegerAnswer(answer.Id);

        // Assert
        Assert.Throws<IntegerAnswerNotFoundException>(() => questionManager.GetIntegerAnswerById(answer.Id));
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


