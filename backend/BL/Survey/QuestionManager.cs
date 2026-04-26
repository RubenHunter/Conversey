using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;
using Conversey.DAL.Survey;

namespace Conversey.BL.Survey;

public class QuestionManager: IQuestionManager
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IProjectManager _projectManager;

    public QuestionManager(IQuestionRepository questionRepository, IProjectManager projectManager)
    {
        _questionRepository = questionRepository;
        _projectManager = projectManager;
    }

    public IEnumerable<Question> GetQuestions(Slug workspaceSlug, Slug projectSlug)
    {
        _ = _projectManager.GetProjectById(workspaceSlug, projectSlug);
        return _questionRepository.ReadQuestionsByProjectIdWithChoices(projectSlug);
    }

    public void SubmitAnswers(
        Slug workspaceSlug,
        Slug projectSlug,
        Guid youthId,
        IEnumerable<(int QuestionId, int? SelectedOptionId, string OpenTextValue)> answers)
    {
        var project = _projectManager.GetProjectById(workspaceSlug, projectSlug);

        Youth youth;
        try
        {
            youth = _projectManager.GetYouth(project, youthId);
        }
        catch (YouthNotFoundException)
        {
            var tempEmail = $"{youthId:N}@{projectSlug.Text}.temp.com";
            youth = _projectManager.AddYouth(youthId, tempEmail, projectSlug);
        }
        

        foreach (var answerInput in answers ?? Array.Empty<(int QuestionId, int? SelectedOptionId, string OpenTextValue)>())
        {
            var question = _questionRepository.ReadQuestionByIdWithProject(answerInput.QuestionId);
            if (question == null || question.Project?.Id != project.Id)
            {
                throw new ValidationException($"Question {answerInput.QuestionId} does not belong to project '{project.Id.Text}'.");
            }

            switch (question)
            {
                case OpenQuestion openQuestion:
                    AddAnswer(new Answer<string>
                    {
                        Value = answerInput.OpenTextValue?.Trim() ?? string.Empty,
                        Question = openQuestion,
                        Youth = youth
                    });
                    break;

                case ScaleQuestion scaleQuestion:
                    SubmitScaleAnswer(scaleQuestion, answerInput, youth);
                    break;

                case ChoiceQuestion<SingleChoice> singleChoiceQuestion:
                    SubmitSingleChoiceAnswer(singleChoiceQuestion, answerInput, youth);
                    break;

                case ChoiceQuestion<MultipleChoice> multipleChoiceQuestion:
                    SubmitMultipleChoiceAnswer(multipleChoiceQuestion, answerInput, youth);
                    break;

                default:
                    throw new ValidationException($"Question {answerInput.QuestionId} has an unsupported type.");
            }
        }
    }

    public Question GetQuestionById(int questionId)
    {
        return _questionRepository.ReadQuestionById(questionId) ?? throw new QuestionNotFoundException(questionId.ToString());
    }

    public IEnumerable<Question> GetAllQuestions()
    {
        return _questionRepository.ReadAllQuestions();
    }

    public Question AddQuestion(Question question)
    {
        Validate(question);
        _questionRepository.CreateQuestion(question);
        return question;
    }

    public Answer GetAnswerById(int answerId)
    {
        return _questionRepository.ReadAnswerById(answerId) ?? throw new AnswerNotFoundException(answerId.ToString());
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

    private void SubmitScaleAnswer(
        ScaleQuestion question,
        (int QuestionId, int? SelectedOptionId, string OpenTextValue) answerInput,
        Youth youth)
    {
        var hasScaleValue = answerInput.SelectedOptionId.HasValue || int.TryParse(answerInput.OpenTextValue, out _);
        if (!hasScaleValue)
        {
            throw new ValidationException($"Question {answerInput.QuestionId} requires a numeric answer.");
        }

        var parsed = answerInput.SelectedOptionId ?? int.Parse(answerInput.OpenTextValue!);
        AddAnswer(new Answer<int>
        {
            Value = parsed,
            Question = question,
            Youth = youth
        });
    }

    private void SubmitSingleChoiceAnswer(
        ChoiceQuestion<SingleChoice> question,
        (int QuestionId, int? SelectedOptionId, string OpenTextValue) answerInput,
        Youth youth)
    {
        if (!answerInput.SelectedOptionId.HasValue)
        {
            throw new ValidationException($"Question {answerInput.QuestionId} requires a selected option.");
        }

        var option = _questionRepository.ReadSingleChoiceByIdForQuestion(answerInput.QuestionId, answerInput.SelectedOptionId.Value);
        if (option == null)
        {
            throw new ValidationException($"Selected option {answerInput.SelectedOptionId.Value} is invalid for question {answerInput.QuestionId}.");
        }

        AddAnswer(new Answer<SingleChoice>
        {
            Value = option,
            Question = question,
            Youth = youth
        });
    }

    private void SubmitMultipleChoiceAnswer(
        ChoiceQuestion<MultipleChoice> question,
        (int QuestionId, int? SelectedOptionId, string OpenTextValue) answerInput,
        Youth youth)
    {
        if (!answerInput.SelectedOptionId.HasValue)
        {
            throw new ValidationException($"Question {answerInput.QuestionId} requires a selected option.");
        }

        var option = _questionRepository.ReadMultipleChoiceByIdForQuestion(answerInput.QuestionId, answerInput.SelectedOptionId.Value);
        if (option == null)
        {
            throw new ValidationException($"Selected option {answerInput.SelectedOptionId.Value} is invalid for question {answerInput.QuestionId}.");
        }

        AddAnswer(new Answer<MultipleChoice>
        {
            Value = option,
            Question = question,
            Youth = youth
        });
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
