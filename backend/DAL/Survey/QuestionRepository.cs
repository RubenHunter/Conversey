using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Survey;

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

    public IReadOnlyCollection<Question> ReadQuestionsByProjectId(Slug projectSlug)
    {
        return _dbContext.Questions
            .Where(q => q.Project.Slug == projectSlug)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Question> ReadQuestionsByProjectIdWithProject(Slug projectSlug)
    {
        return _dbContext.Questions
            .Include(q => q.Project)
            .Where(q => q.Project.Slug == projectSlug)
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

    public Answer ReadAnswerById(int answerId)
    {
        return _dbContext.Answers
            .SingleOrDefault(a => a.Id == answerId);
    }

    public IReadOnlyCollection<Answer> ReadAnswersByQuestionId(int questionId)
    {
        return _dbContext.Answers
            .Where(a => a.Id == questionId)
            .ToList().AsReadOnly();
    }

    public void CreateAnswer(Answer answer)
    {
        _dbContext.Answers.Add(answer);
        _dbContext.SaveChanges();
    }

    public void UpdateAnswer(Answer answer)
    {
        _dbContext.Answers.Update(answer);
        _dbContext.SaveChanges();
    }

    public bool DeleteAnswer(int answerId)
    {
        var answer = _dbContext.Answers
            .SingleOrDefault(a => a.Id == answerId);
        if (answer == null) return false;

        _dbContext.Answers.Remove(answer);
        _dbContext.SaveChanges();
        return true;
    }
}

