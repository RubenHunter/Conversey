using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Survey;
using Conversey.DAL.Survey;

namespace Conversey.BL.Survey;

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

    public IReadOnlyCollection<Question> GetQuestionsByProjectId(int projectId)
    {
        return _questionRepository.ReadQuestionsByProjectId(projectId);
    }

    public IReadOnlyCollection<Question> GetQuestionsByProjectIdWithProject(int projectId)
    {
        return _questionRepository.ReadQuestionsByProjectIdWithProject(projectId);
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

    public TextAnswer GetTextAnswerById(int answerId)
    {
        return _questionRepository.ReadTextAnswerById(answerId) ?? throw new TextAnswerNotFoundException(answerId.ToString());
    }

    public TextAnswer GetTextAnswerByIdWithYouth(int answerId)
    {
        return _questionRepository.ReadTextAnswerByIdWithYouth(answerId) ?? throw new TextAnswerNotFoundException(answerId.ToString());
    }

    public TextAnswer GetTextAnswerByIdWithQuestion(int answerId)
    {
        return _questionRepository.ReadTextAnswerByIdWithQuestion(answerId) ?? throw new TextAnswerNotFoundException(answerId.ToString());
    }

    public TextAnswer GetTextAnswerByIdWithYouthAndQuestion(int answerId)
    {
        return _questionRepository.ReadTextAnswerByIdWithYouthAndQuestion(answerId) ?? throw new TextAnswerNotFoundException(answerId.ToString());
    }

    public IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionId(int questionId)
    {
        return _questionRepository.ReadTextAnswersByQuestionId(questionId);
    }

    public IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionIdWithYouth(int questionId)
    {
        return _questionRepository.ReadTextAnswersByQuestionIdWithYouth(questionId);
    }

    public IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionIdWithQuestion(int questionId)
    {
        return _questionRepository.ReadTextAnswersByQuestionIdWithQuestion(questionId);
    }

    public IReadOnlyCollection<TextAnswer> GetTextAnswersByQuestionIdWithYouthAndQuestion(int questionId)
    {
        return _questionRepository.ReadTextAnswersByQuestionIdWithYouthAndQuestion(questionId);
    }

    public TextAnswer AddTextAnswer(TextAnswer answer)
    {
        Validate(answer);
        _questionRepository.CreateTextAnswer(answer);
        return answer;
    }

    public TextAnswer ChangeTextAnswer(TextAnswer answer)
    {
        Validate(answer);
        _questionRepository.UpdateTextAnswer(answer);
        return answer;
    }

    public void RemoveTextAnswer(int answerId)
    {
        if (!_questionRepository.DeleteTextAnswer(answerId))
        {
            throw new TextAnswerNotFoundException(answerId.ToString());
        }
    }

    public IntegerAnswer GetIntegerAnswerById(int answerId)
    {
        return _questionRepository.ReadIntegerAnswerById(answerId) ?? throw new IntegerAnswerNotFoundException(answerId.ToString());
    }

    public IntegerAnswer GetIntegerAnswerByIdWithYouth(int answerId)
    {
        return _questionRepository.ReadIntegerAnswerByIdWithYouth(answerId) ?? throw new IntegerAnswerNotFoundException(answerId.ToString());
    }

    public IntegerAnswer GetIntegerAnswerByIdWithQuestion(int answerId)
    {
        return _questionRepository.ReadIntegerAnswerByIdWithQuestion(answerId) ?? throw new IntegerAnswerNotFoundException(answerId.ToString());
    }

    public IntegerAnswer GetIntegerAnswerByIdWithYouthAndQuestion(int answerId)
    {
        return _questionRepository.ReadIntegerAnswerByIdWithYouthAndQuestion(answerId) ?? throw new IntegerAnswerNotFoundException(answerId.ToString());
    }

    public IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionId(int questionId)
    {
        return _questionRepository.ReadIntegerAnswersByQuestionId(questionId);
    }

    public IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionIdWithYouth(int questionId)
    {
        return _questionRepository.ReadIntegerAnswersByQuestionIdWithYouth(questionId);
    }

    public IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionIdWithQuestion(int questionId)
    {
        return _questionRepository.ReadIntegerAnswersByQuestionIdWithQuestion(questionId);
    }

    public IReadOnlyCollection<IntegerAnswer> GetIntegerAnswersByQuestionIdWithYouthAndQuestion(int questionId)
    {
        return _questionRepository.ReadIntegerAnswersByQuestionIdWithYouthAndQuestion(questionId);
    }

    public IntegerAnswer AddIntegerAnswer(IntegerAnswer answer)
    {
        Validate(answer);
        _questionRepository.CreateIntegerAnswer(answer);
        return answer;
    }

    public IntegerAnswer ChangeIntegerAnswer(IntegerAnswer answer)
    {
        Validate(answer);
        _questionRepository.UpdateIntegerAnswer(answer);
        return answer;
    }

    public void RemoveIntegerAnswer(int answerId)
    {
        if (!_questionRepository.DeleteIntegerAnswer(answerId))
        {
            throw new IntegerAnswerNotFoundException(answerId.ToString());
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
