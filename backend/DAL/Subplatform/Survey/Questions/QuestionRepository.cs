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
            .SingleOrDefault(q => q.Id == questionId);
    }

    public Question ReadQuestionByIdWithProject(int questionId)
    {
        return _dbContext.Questions
            .Include(q => q.Project)
            .SingleOrDefault(q => q.Id == questionId);
    }

    public IReadOnlyCollection<Question> ReadAllQuestions()
    {
        return _dbContext.Questions.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Question> ReadAllQuestionsWithProject()
    {
        return _dbContext.Questions
            .Include(q => q.Project)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Question> ReadQuestionsByProjectId(int projectId)
    {
        return _dbContext.Questions
            .Where(q => q.Project.Id == projectId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Question> ReadQuestionsByProjectIdWithProject(int projectId)
    {
        return _dbContext.Questions
            .Include(q => q.Project)
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

    public bool DeleteQuestion(int questionId)
    {
        var question = _dbContext.Questions
            .SingleOrDefault(q => q.Id == questionId);
        if (question == null) return false;

        _dbContext.Questions.Remove(question);
        _dbContext.SaveChanges();
        return true;
    }

    public TextAnswer ReadTextAnswerById(int answerId)
    {
        return _dbContext.TextAnswers
            .SingleOrDefault(a => a.Id == answerId);
    }

    public TextAnswer ReadTextAnswerByIdWithYouth(int answerId)
    {
        return _dbContext.TextAnswers
            .Include(a => a.Youth)
            .SingleOrDefault(a => a.Id == answerId);
    }

    public TextAnswer ReadTextAnswerByIdWithQuestion(int answerId)
    {
        return _dbContext.TextAnswers
            .Include(a => a.Question)
            .SingleOrDefault(a => a.Id == answerId);
    }

    public TextAnswer ReadTextAnswerByIdWithYouthAndQuestion(int answerId)
    {
        return _dbContext.TextAnswers
            .Include(a => a.Youth)
            .Include(a => a.Question)
            .SingleOrDefault(a => a.Id == answerId);
    }

    public IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionId(int questionId)
    {
        return _dbContext.TextAnswers
            .Where(a => a.QuestionId == questionId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionIdWithYouth(int questionId)
    {
        return _dbContext.TextAnswers
            .Include(a => a.Youth)
            .Where(a => a.QuestionId == questionId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionIdWithQuestion(int questionId)
    {
        return _dbContext.TextAnswers
            .Include(a => a.Question)
            .Where(a => a.QuestionId == questionId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<TextAnswer> ReadTextAnswersByQuestionIdWithYouthAndQuestion(int questionId)
    {
        return _dbContext.TextAnswers
            .Include(a => a.Youth)
            .Include(a => a.Question)
            .Where(a => a.QuestionId == questionId)
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

    public bool DeleteTextAnswer(int answerId)
    {
        var answer = _dbContext.TextAnswers
            .SingleOrDefault(a => a.Id == answerId);
        if (answer == null) return false;

        _dbContext.TextAnswers.Remove(answer);
        _dbContext.SaveChanges();
        return true;
    }

    public IntegerAnswer ReadIntegerAnswerById(int answerId)
    {
        return _dbContext.IntegerAnswers
            .SingleOrDefault(a => a.Id == answerId);
    }

    public IntegerAnswer ReadIntegerAnswerByIdWithYouth(int answerId)
    {
        return _dbContext.IntegerAnswers
            .Include(a => a.Youth)
            .SingleOrDefault(a => a.Id == answerId);
    }

    public IntegerAnswer ReadIntegerAnswerByIdWithQuestion(int answerId)
    {
        return _dbContext.IntegerAnswers
            .Include(a => a.Question)
            .SingleOrDefault(a => a.Id == answerId);
    }

    public IntegerAnswer ReadIntegerAnswerByIdWithYouthAndQuestion(int answerId)
    {
        return _dbContext.IntegerAnswers
            .Include(a => a.Youth)
            .Include(a => a.Question)
            .SingleOrDefault(a => a.Id == answerId);
    }

    public IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionId(int questionId)
    {
        return _dbContext.IntegerAnswers
            .Where(a => a.QuestionId == questionId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionIdWithYouth(int questionId)
    {
        return _dbContext.IntegerAnswers
            .Include(a => a.Youth)
            .Where(a => a.QuestionId == questionId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionIdWithQuestion(int questionId)
    {
        return _dbContext.IntegerAnswers
            .Include(a => a.Question)
            .Where(a => a.QuestionId == questionId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<IntegerAnswer> ReadIntegerAnswersByQuestionIdWithYouthAndQuestion(int questionId)
    {
        return _dbContext.IntegerAnswers
            .Include(a => a.Youth)
            .Include(a => a.Question)
            .Where(a => a.QuestionId == questionId)
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

    public bool DeleteIntegerAnswer(int answerId)
    {
        var answer = _dbContext.IntegerAnswers
            .SingleOrDefault(a => a.Id == answerId);
        if (answer == null) return false;

        _dbContext.IntegerAnswers.Remove(answer);
        _dbContext.SaveChanges();
        return true;
    }
}
