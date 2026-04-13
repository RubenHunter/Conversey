using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;
using Conversey.DAL.Survey;

namespace Conversey.BL.Subplatform.Survey.Questions;

public class QuestionManager: IQuestionManager
{
    private readonly IQuestionRepository _questionRepository;

    public QuestionManager(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public Question GetQuestionById(int questionId)
    {
        return _questionRepository.ReadQuestionById(questionId) ?? throw new QuestionNotFoundException(questionId.ToString());
    }

    public Question GetQuestionByIdWithProject(int questionId)
    {
        return _questionRepository.ReadQuestionByIdWithProject(questionId) ?? throw new QuestionNotFoundException(questionId.ToString());
    }

    public IReadOnlyCollection<Question> GetAllQuestions()
    {
        return _questionRepository.ReadAllQuestions();
    }

    public IReadOnlyCollection<Question> GetAllQuestionsWithProject()
    {
        return _questionRepository.ReadAllQuestionsWithProject();
    }

    public IReadOnlyCollection<Question> GetQuestionsByProjectId(Slug projectSlug)
    {
        return _questionRepository.ReadQuestionsByProjectId(projectSlug);
    }

    public IReadOnlyCollection<Question> GetQuestionsByProjectIdWithProject(Slug projectSlug)
    {
        return _questionRepository.ReadQuestionsByProjectIdWithProject(projectSlug);
    }

    public Question AddQuestion(Question question)
    {
        Validate(question);
        _questionRepository.CreateQuestion(question);
        return question;
    }

    public Question ChangeQuestion(Question question)
    {
        Validate(question);
        _questionRepository.UpdateQuestion(question);
        return question;
    }

    public void RemoveQuestion(int questionId)
    {
        if (!_questionRepository.DeleteQuestion(questionId))
        {
            throw new QuestionNotFoundException(questionId.ToString());
        }
    }

    public Answer GetAnswerById(int answerId)
    {
        return _questionRepository.ReadAnswerById(answerId) ?? throw new AnswerNotFoundException(answerId.ToString());
    }

    public IReadOnlyCollection<Answer> GetAnswersByQuestionId(int questionId)
    {
        return _questionRepository.ReadAnswersByQuestionId(questionId);
    }

    public Answer AddAnswer(Answer answer)
    {
        Validate(answer);
        _questionRepository.CreateAnswer(answer);
        return answer;
    }

    public Answer ChangeAnswer(Answer answer)
    {
        Validate(answer);
        _questionRepository.UpdateAnswer(answer);
        return answer;
    }

    public void RemoveAnswer(int answerId)
    {
        if (!_questionRepository.DeleteAnswer(answerId))
        {
            throw new AnswerNotFoundException(answerId.ToString());
        }
    }

    private void Validate(object obj)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(obj);

        if (!Validator.TryValidateObject(obj, context, validationResults, true))
        {
            throw new ValidationException(string.Join("; ", validationResults.Select(r => r.ErrorMessage)));
        }
    }
}
