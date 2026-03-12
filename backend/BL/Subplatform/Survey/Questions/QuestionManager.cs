using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;
using Conversey.DAL.Subplatform.Survey.Questions;

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
        return _questionRepository.ReadQuestionById(questionId);
    }

    public IReadOnlyCollection<Question> GetAllQuestions()
    {
        return _questionRepository.ReadAllQuestions();
    }

    public IReadOnlyCollection<Question> GetQuestionsByProjectId(int projectId)
    {
        return _questionRepository.ReadQuestionsByProjectId(projectId);
    }

    public Question AddQuestion(Question question)
    {
        Validate(question);
        _questionRepository.CreateQuestion(question);
        return question;
    }

    public Question EditQuestion(Question question)
    {
        Validate(question);
        _questionRepository.UpdateQuestion(question);
        return question;
    }

    public void RemoveQuestion(int questionId)
    {
        _questionRepository.DeleteQuestion(questionId);
    }

    public TextAnswer GetTextAnswerById(int answerId)
    {
        return _questionRepository.ReadTextAnswerById(answerId);
    }

    public IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionId(int questionId)
    {
        return _questionRepository.ReadTextAnswersByQuestionId(questionId);
    }

    public TextAnswer AddTextAnswer(TextAnswer answer)
    {
        Validate(answer);
        _questionRepository.CreateTextAnswer(answer);
        return answer;
    }

    public TextAnswer EditTextAnswer(TextAnswer answer)
    {
        Validate(answer);
        _questionRepository.UpdateTextAnswer(answer);
        return answer;
    }

    public void RemoveTextAnswer(int answerId)
    {
        _questionRepository.DeleteTextAnswer(answerId);
    }

    public IntegerAnswer GetIntegerAnswerById(int answerId)
    {
        return _questionRepository.ReadIntegerAnswerById(answerId);
    }

    public IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionId(int questionId)
    {
        return _questionRepository.ReadIntegerAnswersByQuestionId(questionId);
    }

    public IntegerAnswer AddIntegerAnswer(IntegerAnswer answer)
    {
        Validate(answer);
        _questionRepository.CreateIntegerAnswer(answer);
        return answer;
    }

    public IntegerAnswer EditIntegerAnswer(IntegerAnswer answer)
    {
        Validate(answer);
        _questionRepository.UpdateIntegerAnswer(answer);
        return answer;
    }

    public void RemoveIntegerAnswer(int answerId)
    {
        _questionRepository.DeleteIntegerAnswer(answerId);
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

