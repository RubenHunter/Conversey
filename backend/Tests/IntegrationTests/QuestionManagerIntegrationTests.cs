using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Questions;
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

    [Fact]
    public void GetAllQuestions_ShouldReturnSeededQuestions()
    {
        using var scope = _fixture.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();

        var questions = manager.GetAllQuestions();

        Assert.NotEmpty(questions);
    }

    [Fact]
    public void AddQuestion_And_GetById_ShouldPersistQuestion()
    {
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var project = projectManager.GetProjectBySlug(ManagerSeedData.ProjectSlug);

        var created = questionManager.AddQuestion(new OpenQuestion
        {
            Text = "Integration question",
            Required = true,
            Project = project
        });

        var loaded = questionManager.GetQuestionById(created.Id);
        Assert.Equal("Integration question", loaded.Text);
    }

    [Fact]
    public void AddAnswer_ChangeAnswer_RemoveAnswer_ShouldWork()
    {
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();

        var project = projectManager.GetProjectBySlug(ManagerSeedData.ProjectSlug);
        var youth = projectManager.GetYouthByToken(ManagerSeedData.YouthToken);

        var question = questionManager.AddQuestion(new OpenQuestion
        {
            Text = "Answerable question",
            Required = true,
            Project = project
        });

        var typedQuestion = questionManager.GetQuestionById(question.Id) as Question<Answer<string>>;
        Assert.NotNull(typedQuestion);

        var created = questionManager.AddAnswer(new Answer<string>
        {
            Value = "Initial value",
            Question = typedQuestion,
            Youth = youth
        });

        var loaded = questionManager.GetAnswerById(created.Id);
        Assert.Equal(created.Id, loaded.Id);

        var editable = created as Answer<string>;
        Assert.NotNull(editable);
        editable.Value = "Updated value";
        var changed = questionManager.ChangeAnswer(editable);
        Assert.Equal("Updated value", ((Answer<string>)changed).Value);

        questionManager.RemoveAnswer(created.Id);
        Assert.Throws<AnswerNotFoundException>(() => questionManager.GetAnswerById(created.Id));
    }

    [Fact]
    public void AddQuestion_WhenInvalid_ShouldThrowValidationException()
    {
        using var scope = _fixture.CreateScope();
        var questionManager = scope.ServiceProvider.GetRequiredService<IQuestionManager>();
        var projectManager = scope.ServiceProvider.GetRequiredService<IProjectManager>();
        var project = projectManager.GetProjectBySlug(ManagerSeedData.ProjectSlug);

        var act = () => questionManager.AddQuestion(new OpenQuestion
        {
            Text = string.Empty,
            Required = true,
            Project = project
        });

        Assert.Throws<ValidationException>(act);
    }
}
