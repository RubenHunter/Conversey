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

    public QuestionManager(IQuestionRepository questionRepository)
    {
        _questionRepository = questionRepository;
    }

    public IEnumerable<Question> GetQuestions(Slug workspaceSlug, Slug projectSlug)
    {
        _ = GetProjectForWorkspace(workspaceSlug, projectSlug);
        return _questionRepository.ReadQuestionsByProjectIdWithChoices(projectSlug);
    }

    public void SubmitAnswers(
        Slug workspaceSlug,
        Slug projectSlug,
        string youthId,
        IEnumerable<(int QuestionId, int? SelectedOptionId, string OpenTextValue)> answers)
    {
        var project = GetProjectForWorkspace(workspaceSlug, projectSlug);

        if (!Guid.TryParse(youthId?.Trim(), out var youthToken))
        {
            throw new ValidationException("YouthId must be a valid GUID.");
        }

        var youth = ResolveYouth(project, youthToken);

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

    private Project GetProjectForWorkspace(Slug workspaceSlug, Slug projectId)
    {
        var project = _questionRepository.ReadProjectBySlugWithWorkspaceAndQuestions(projectId)
                      ?? throw new ProjectNotFoundException(projectId);

        if (project.Workspace?.Id != workspaceSlug)
        {
            throw new ProjectNotFoundException(projectId);
        }

        return project;
    }

    private Youth ResolveYouth(Project project, Guid youthToken)
    {
        var youth = _questionRepository.ReadYouthByTokenWithProject(youthToken);
        if (youth == null)
        {
            return _questionRepository.CreateYouth(youthToken, project.Id);
        }

        if (youth.Project?.Id != project.Id)
        {
            throw new ValidationException($"Youth '{youthToken}' does not belong to project '{project.Id.Text}'.");
        }

        return youth;
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
