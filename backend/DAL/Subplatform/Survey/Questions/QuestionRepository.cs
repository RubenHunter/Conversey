using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Survey.Questions;

public class QuestionRepository : IQuestionRepository
{
    private readonly ConverseyDbContext _dbContext;

    public QuestionRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Question ReadQuestionById(int questionId)
    {
        return _dbContext.Questions
            .FirstOrDefault(q => q.Id == questionId)
            ?? throw new KeyNotFoundException($"Question with id {questionId} not found.");
    }

    public IReadOnlyCollection<Question> ReadAllQuestions()
    {
        return _dbContext.Questions.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Question> ReadQuestionsByProjectId(int projectId)
    {
        return _dbContext.Questions
            .Where(q => q.Project.Id == projectId)
            .ToList().AsReadOnly();
    }

    public void CreateQuestion(Question question)
    {
        _dbContext.Questions.Add(question);
        _dbContext.SaveChanges();
    }

    public void UpdateQuestion(Question question)
    {
        _dbContext.Questions.Update(question);
        _dbContext.SaveChanges();
    }

    public void DeleteQuestion(int questionId)
    {
        var question = _dbContext.Questions.Find(questionId)
            ?? throw new KeyNotFoundException($"Question with id {questionId} not found.");
        _dbContext.Questions.Remove(question);
        _dbContext.SaveChanges();
    }

    public TextAnswer ReadTextAnswerById(int answerId)
    {
        return _dbContext.TextAnswers
            .Include(a => a.Youth)
            .FirstOrDefault(a => a.Id == answerId)
            ?? throw new KeyNotFoundException($"TextAnswer with id {answerId} not found.");
    }

    public IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionId(int questionId)
    {
        return _dbContext.TextAnswers
            .Where(a => a.Question.Id == questionId)
            .ToList().AsReadOnly();
    }

    public void CreateTextAnswer(TextAnswer answer)
    {
        _dbContext.TextAnswers.Add(answer);
        _dbContext.SaveChanges();
    }

    public void UpdateTextAnswer(TextAnswer answer)
    {
        _dbContext.TextAnswers.Update(answer);
        _dbContext.SaveChanges();
    }

    public void DeleteTextAnswer(int answerId)
    {
        var answer = _dbContext.TextAnswers.Find(answerId)
            ?? throw new KeyNotFoundException($"TextAnswer with id {answerId} not found.");
        _dbContext.TextAnswers.Remove(answer);
        _dbContext.SaveChanges();
    }

    public IntegerAnswer ReadIntegerAnswerById(int answerId)
    {
        return _dbContext.IntegerAnswers
            .Include(a => a.Youth)
            .FirstOrDefault(a => a.Id == answerId)
            ?? throw new KeyNotFoundException($"IntegerAnswer with id {answerId} not found.");
    }

    public IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionId(int questionId)
    {
        return _dbContext.IntegerAnswers
            .Where(a => a.Question.Id == questionId)
            .ToList().AsReadOnly();
    }

    public void CreateIntegerAnswer(IntegerAnswer answer)
    {
        _dbContext.IntegerAnswers.Add(answer);
        _dbContext.SaveChanges();
    }

    public void UpdateIntegerAnswer(IntegerAnswer answer)
    {
        _dbContext.IntegerAnswers.Update(answer);
        _dbContext.SaveChanges();
    }

    public void DeleteIntegerAnswer(int answerId)
    {
        var answer = _dbContext.IntegerAnswers.Find(answerId)
            ?? throw new KeyNotFoundException($"IntegerAnswer with id {answerId} not found.");
        _dbContext.IntegerAnswers.Remove(answer);
        _dbContext.SaveChanges();
    }
}
