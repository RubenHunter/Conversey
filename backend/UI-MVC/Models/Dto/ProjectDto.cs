using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;

namespace Conversey.UI_MVC.Models.Dto;

public class ProjectDto
{
    public Slug Id { get; set; }
    public Slug OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string InteractionForm { get; set; }
    public string Language { get; set; }
    public ProjectTopicDto Topic { get; set; }
    public IEnumerable<ProjectTopicDto> Topics { get; set; } = Array.Empty<ProjectTopicDto>();

    public static ProjectDto From(Project project)
    {
        var firstTopic = project.Topic?.FirstOrDefault();
        IReadOnlyCollection<ProjectTopicDto> topics = (project.Topic ?? Array.Empty<Topic>())
            .Select(ProjectTopicDto.From)
            .ToList()
            .AsReadOnly();

        return new ProjectDto
        {
            Id = project.Id,
            OrganizationId = project.Workspace.Id,
            OrganizationName = project.Workspace.Name,
            Title = project.Name,
            Description = project.Description ?? string.Empty,
            ImageUrl = project.ImageUrl ?? string.Empty,
            Status = project.Status.ToString(),
            StartDate = project.StartDate == default ? null : project.StartDate,
            EndDate = project.EndDate == default ? null : project.EndDate,
            InteractionForm = project.InteractionForm.ToString(),
            Language = project.Language ?? "nl",
            Topic = firstTopic is null ? null : ProjectTopicDto.From(firstTopic),
            Topics = topics
        };
    }
}

public class ProjectTopicDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Context { get; set; }
    public int MaxBroadSelectionLoads { get; set; }

    public static ProjectTopicDto From(Topic topic)
    {
        return new ProjectTopicDto
        {
            Id = topic.Id,
            Name = topic.Name,
            Context = topic.Context ?? string.Empty,
            MaxBroadSelectionLoads = topic.MaxBroadSelectionLoads
        };
    }
}

public class QuestionDto
{
    public int Id { get; set; }
    public Slug ProjectId { get; set; }
    public string Text { get; set; }
    public bool IsRequired { get; set; }
    public string Type { get; set; }
    public IEnumerable<AnswerOptionDto> Options { get; set; } = Array.Empty<AnswerOptionDto>();

    public static QuestionDto From(Question question, Slug projectId, IReadOnlyCollection<AnswerOptionDto> options = null)
    {
        static IReadOnlyCollection<AnswerOptionDto> MapSingleChoiceOptions(ChoiceQuestion<SingleChoice> q)
        {
            return (q.PossibleChoices ?? Array.Empty<SingleChoice>())
                .Select(choice => new AnswerOptionDto
                {
                    Id = choice.Id,
                    QuestionId = q.Id,
                    Text = choice.Text
                })
                .ToList()
                .AsReadOnly();
        }

        static IReadOnlyCollection<AnswerOptionDto> MapMultipleChoiceOptions(ChoiceQuestion<MultipleChoice> q)
        {
            return (q.PossibleChoices ?? Array.Empty<MultipleChoice>())
                .Select(choice => new AnswerOptionDto
                {
                    Id = choice.Id,
                    QuestionId = q.Id,
                    Text = choice.Text
                })
                .ToList()
                .AsReadOnly();
        }

        return new QuestionDto
        {
            Id = question.Id,
            ProjectId = projectId,
            Text = question.Text,
            IsRequired = question.Required,
            Type = question switch
            {
                ChoiceQuestion<SingleChoice> => "SingleChoice",
                ChoiceQuestion<MultipleChoice> => "MultipleChoice",
                ScaleQuestion => "Scale",
                OpenQuestion => "OpenText",
                _ => "Choice"
            },
            Options = options ?? question switch
            {
                ChoiceQuestion<SingleChoice> singleChoiceQuestion => MapSingleChoiceOptions(singleChoiceQuestion),
                ChoiceQuestion<MultipleChoice> multipleChoiceQuestion => MapMultipleChoiceOptions(multipleChoiceQuestion),
                _ => Array.Empty<AnswerOptionDto>()
            }
        };
    }
}

public class AnswerOptionDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; }
}
