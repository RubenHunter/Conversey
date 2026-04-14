using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Questions;
using Conversey.DAL;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceSlug}/projects/{projectSlug}")]
public class QuestionController : ControllerBase
{
    private readonly IProjectManager _projectManager;
    private readonly IQuestionManager _questionManager;
    private readonly ConverseyDbContext _dbContext;

    public QuestionController(IProjectManager projectManager, IQuestionManager questionManager, ConverseyDbContext dbContext)
    {
        _projectManager = projectManager;
        _questionManager = questionManager;
        _dbContext = dbContext;
    }

    [HttpGet("questions")]
    public ActionResult<IReadOnlyCollection<QuestionDto>> GetQuestions(string workspaceSlug, string projectSlug)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            var choiceQuestionIds = (project.Questions ?? Array.Empty<Question>())
                .Where(question => question is ChoiceQuestion<SingleChoice> || question is ChoiceQuestion<MultipleChoice>)
                .Select(question => question.Id)
                .ToHashSet();

            var singleOptions = _dbContext.Set<SingleChoice>()
                .Where(choice => choiceQuestionIds.Contains(EF.Property<int>(choice, "QuestionId")))
                .Select(choice => new
                {
                    QuestionId = EF.Property<int>(choice, "QuestionId"),
                    Option = new AnswerOptionDto
                    {
                        Id = choice.Id,
                        QuestionId = EF.Property<int>(choice, "QuestionId"),
                        Text = choice.Text
                    }
                })
                .AsEnumerable()
                .GroupBy(entry => entry.QuestionId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyCollection<AnswerOptionDto>)group.Select(entry => entry.Option).ToList().AsReadOnly());

            var multipleOptions = _dbContext.Set<MultipleChoice>()
                .Where(choice => choiceQuestionIds.Contains(EF.Property<int>(choice, "QuestionId")))
                .Select(choice => new
                {
                    QuestionId = EF.Property<int>(choice, "QuestionId"),
                    Option = new AnswerOptionDto
                    {
                        Id = choice.Id,
                        QuestionId = EF.Property<int>(choice, "QuestionId"),
                        Text = choice.Text
                    }
                })
                .AsEnumerable()
                .GroupBy(entry => entry.QuestionId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyCollection<AnswerOptionDto>)group.Select(entry => entry.Option).ToList().AsReadOnly());

            var dtos = (project.Questions ?? Array.Empty<Question>())
                .Select(question =>
                {
                    if (singleOptions.TryGetValue(question.Id, out var singleChoiceOptions))
                    {
                        return QuestionDto.From(question, project.Slug.Text, singleChoiceOptions);
                    }

                    if (multipleOptions.TryGetValue(question.Id, out var multipleChoiceOptions))
                    {
                        return QuestionDto.From(question, project.Slug.Text, multipleChoiceOptions);
                    }

                    return QuestionDto.From(question, project.Slug.Text);
                })
                .ToList()
                .AsReadOnly();

            return Ok(dtos);
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("answers")]
    public ActionResult SubmitAnswers(string workspaceSlug, string projectSlug, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);

            if (!Guid.TryParse(submission.YouthId?.Trim(), out var youthToken))
            {
                return BadRequest("YouthId must be a valid GUID.");
            }

            var youth = ResolveYouth(project, youthToken);

            foreach (var answerDto in submission.Answers)
            {
                var question = _questionManager.GetQuestionById(answerDto.QuestionId);
                var belongsToProject = question.Project?.Slug == project.Slug || (project.Questions?.Any(q => q.Id == question.Id) ?? false);
                if (!belongsToProject)
                {
                    return BadRequest($"Question {answerDto.QuestionId} does not belong to project '{project.Slug.Text}'.");
                }

                switch (question)
                {
                    case OpenQuestion openQuestion:
                    {
                        var openAnswer = new Answer<string>
                        {
                            Value = answerDto.OpenTextValue?.Trim() ?? string.Empty,
                            Question = openQuestion,
                            Youth = youth
                        };
                        _questionManager.AddAnswer(openAnswer);
                        break;
                    }
                    case ScaleQuestion scaleQuestion:
                    {
                        var hasScaleValue = answerDto.SelectedOptionId.HasValue || int.TryParse(answerDto.OpenTextValue, out _);
                        if (!hasScaleValue)
                        {
                            return BadRequest($"Question {answerDto.QuestionId} requires a numeric answer.");
                        }

                        var parsed = answerDto.SelectedOptionId ?? int.Parse(answerDto.OpenTextValue!);
                        var scaleAnswer = new Answer<int>
                        {
                            Value = parsed,
                            Question = scaleQuestion,
                            Youth = youth
                        };
                        _questionManager.AddAnswer(scaleAnswer);
                        break;
                    }
                    case ChoiceQuestion<SingleChoice> singleChoiceQuestion:
                    {
                        if (!answerDto.SelectedOptionId.HasValue)
                        {
                            return BadRequest($"Question {answerDto.QuestionId} requires a selected option.");
                        }

                        var option = _dbContext.Set<SingleChoice>()
                            .SingleOrDefault(choice => choice.Id == answerDto.SelectedOptionId.Value &&
                                                     EF.Property<int>(choice, "QuestionId") == answerDto.QuestionId);

                        if (option == null)
                        {
                            return BadRequest($"Selected option {answerDto.SelectedOptionId.Value} is invalid for question {answerDto.QuestionId}.");
                        }

                        var singleChoiceAnswer = new Answer<SingleChoice>
                        {
                            Value = option,
                            Question = singleChoiceQuestion,
                            Youth = youth
                        };
                        _questionManager.AddAnswer(singleChoiceAnswer);
                        break;
                    }
                    case ChoiceQuestion<MultipleChoice> multipleChoiceQuestion:
                    {
                        if (!answerDto.SelectedOptionId.HasValue)
                        {
                            return BadRequest($"Question {answerDto.QuestionId} requires a selected option.");
                        }

                        var option = _dbContext.Set<MultipleChoice>()
                            .SingleOrDefault(choice => choice.Id == answerDto.SelectedOptionId.Value &&
                                                     EF.Property<int>(choice, "QuestionId") == answerDto.QuestionId);

                        if (option == null)
                        {
                            return BadRequest($"Selected option {answerDto.SelectedOptionId.Value} is invalid for question {answerDto.QuestionId}.");
                        }

                        var multipleChoiceAnswer = new Answer<MultipleChoice>
                        {
                            Value = option,
                            Question = multipleChoiceQuestion,
                            Youth = youth
                        };
                        _questionManager.AddAnswer(multipleChoiceAnswer);
                        break;
                    }
                }
            }

            return NoContent();
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException e)
        {
            return BadRequest(e.Message);
        }
    }

    // Backward-compatible alias while frontend switches from /responses to /answers.
    [HttpPost("responses")]
    public ActionResult SubmitResponsesAlias(string workspaceSlug, string projectSlug, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        return SubmitAnswers(workspaceSlug, projectSlug, submission);
    }

    private Project GetProjectForWorkspace(string workspaceSlug, string projectSlug)
    {
        var project = _projectManager.GetProjectBySlugWithWorkspaceAndQuestions(ToSlug(projectSlug));

        if (!string.Equals(project.Workspace.Id.Text, workspaceSlug, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProjectNotFoundException($"{workspaceSlug}/{projectSlug}");
        }

        return project;
    }

    private Youth ResolveYouth(Project project, Guid youthToken)
    {
        var youth = _dbContext.Youths
            .Include(y => y.Project)
            .SingleOrDefault(y => y.Token == youthToken);

        if (youth == null)
        {
            _projectManager.AddYouth(youthToken, null, project.Slug);
            youth = _dbContext.Youths
                .Include(y => y.Project)
                .Single(y => y.Token == youthToken);
        }

        if (youth.Project?.Slug != project.Slug)
        {
            throw new ValidationException($"Youth '{youthToken}' does not belong to project '{project.Slug.Text}'.");
        }

        return youth;
    }

    private static Slug ToSlug(string value)
    {
        return new Slug { Text = value.Trim().ToLowerInvariant() };
    }
}
