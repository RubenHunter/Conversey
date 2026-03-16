using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Questions;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Questions;
using Conversey.REST.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.REST.Controllers.Api;

[ApiController]
[Route("api/workspaces/{workspaceSlug}/projects/{projectSlug}")]
public class QuestionController : ControllerBase
{
    private readonly IProjectManager _projectManager;
    private readonly IQuestionManager _questionManager;

    public QuestionController(IProjectManager projectManager, IQuestionManager questionManager)
    {
        _projectManager = projectManager;
        _questionManager = questionManager;
    }

    [HttpGet("questions")]
    public ActionResult<IReadOnlyCollection<QuestionDto>> GetQuestions(string workspaceSlug, string projectSlug)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);
            IReadOnlyCollection<QuestionDto> dtos = (project.Questions ?? Array.Empty<Question>())
                .OrderBy(question => question.Order)
                .ThenBy(question => question.Id)
                .Select(question => QuestionDto.From(question, project.Id))
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
        return SubmitAnswersInternal(workspaceSlug, projectSlug, submission);
    }

    // Backward-compatible alias while frontend switches from /responses to /answers.
    [HttpPost("responses")]
    public ActionResult SubmitResponsesAlias(string workspaceSlug, string projectSlug, [FromBody] SurveyAnswerSubmissionRequestDto submission)
    {
        return SubmitAnswersInternal(workspaceSlug, projectSlug, submission);
    }

    private ActionResult SubmitAnswersInternal(string workspaceSlug, string projectSlug, SurveyAnswerSubmissionRequestDto submission)
    {
        try
        {
            var project = GetProjectForWorkspace(workspaceSlug, projectSlug);

            if (submission.ProjectId != project.Id)
            {
                return BadRequest("ProjectId in payload does not match route project.");
            }

            if (string.IsNullOrWhiteSpace(submission.YouthId))
            {
                return BadRequest("YouthId is required.");
            }

            Youth youth;
            try
            {
                youth = _projectManager.GetYouthByToken(submission.YouthId);
            }
            catch (YouthNotFoundException)
            {
                youth = _projectManager.AddYouth(submission.YouthId, string.Empty, project.Id);
            }

            var questionsById = (project.Questions ?? Array.Empty<Question>())
                .ToDictionary(question => question.Id);

            foreach (var answer in submission.Answers)
            {
                if (!questionsById.TryGetValue(answer.QuestionId, out var question))
                {
                    return BadRequest($"Question {answer.QuestionId} does not belong to this project.");
                }

                bool hasOptions = question.Options.Count > 0;
                bool hasOpenText = !string.IsNullOrWhiteSpace(answer.OpenTextValue);
                bool hasSelectedOption = answer.SelectedOptionId.HasValue;

                if (hasOpenText)
                {
                    if (hasOptions)
                    {
                        return BadRequest($"Question {question.Id} expects a selected option.");
                    }

                    _questionManager.AddTextAnswer(new OpenTextAnswer
                    {
                        YouthToken = youth.Token,
                        Youth = youth,
                        QuestionId = question.Id,
                        Question = question,
                        Value = answer.OpenTextValue.Trim()
                    });

                    continue;
                }

                if (hasSelectedOption)
                {
                    if (question is ScaleQuestion)
                    {
                        _questionManager.AddIntegerAnswer(new IntegerAnswer
                        {
                            YouthToken = youth.Token,
                            Youth = youth,
                            QuestionId = question.Id,
                            Question = question,
                            Value = answer.SelectedOptionId.Value
                        });

                        continue;
                    }

                    if (!hasOptions)
                    {
                        return BadRequest($"Question {question.Id} expects an open text answer.");
                    }

                    var selectedOption = question.Options.FirstOrDefault(option => option.Id == answer.SelectedOptionId);
                    if (selectedOption is null)
                    {
                        return BadRequest($"Selected option {answer.SelectedOptionId} is invalid for question {question.Id}.");
                    }

                    _questionManager.AddTextAnswer(new ClosedTextAnswer
                    {
                        YouthToken = youth.Token,
                        Youth = youth,
                        QuestionId = question.Id,
                        Question = question,
                        Value = selectedOption.Text
                    });

                    continue;
                }

                return BadRequest($"Answer for question {question.Id} is missing content.");
            }

            return NoContent();
        }
        catch (ProjectNotFoundException)
        {
            return NotFound();
        }
    }

    private Project GetProjectForWorkspace(string workspaceSlug, string projectSlug)
    {
        var project = _projectManager.GetProjectBySlugWithWorkspaceAndQuestions(ToSlug(projectSlug));

        if (!string.Equals(project.Workspace.Slug.Text, workspaceSlug, StringComparison.OrdinalIgnoreCase))
        {
            throw new ProjectNotFoundException($"{workspaceSlug}/{projectSlug}");
        }

        return project;
    }

    private static Slug ToSlug(string value)
    {
        return new Slug { Text = value.Trim().ToLowerInvariant() };
    }
}
