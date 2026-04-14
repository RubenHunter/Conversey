using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Survey;

namespace Conversey.REST.Models.Dto;

public class ProjectDto
{
    public int Id { get; set; }
    public string Slug { get; set; }
    public string OrganizationSlug { get; set; }
    public string OrganizationName { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string InteractionForm { get; set; }
    public ProjectTopicDto Topic { get; set; }
    public IReadOnlyCollection<ProjectTopicDto> Topics { get; set; } = Array.Empty<ProjectTopicDto>();

    public static ProjectDto From(Project project)
    {
        static int ToStableProjectId(string slug)
        {
            unchecked
            {
                var hash = 17;
                foreach (var ch in slug)
                {
                    hash = (hash * 31) + ch;
                }

                return Math.Abs(hash == int.MinValue ? int.MaxValue : hash);
            }
        }

        var firstTopic = project.Topic?.FirstOrDefault();
        IReadOnlyCollection<ProjectTopicDto> topics = (project.Topic ?? Array.Empty<Topic>())
            .Select(ProjectTopicDto.From)
            .ToList()
            .AsReadOnly();

        return new ProjectDto
        {
            Id = ToStableProjectId(project.Slug.Text),
            Slug = project.Slug.Text,
            OrganizationSlug = project.Workspace.Id.Text,
            OrganizationName = project.Workspace.Name,
            Title = project.Name,
            Description = project.Description ?? string.Empty,
            ImageUrl = project.ImageUrl ?? string.Empty,
            Status = project.Status.ToString(),
            StartDate = project.StartDate == default ? null : project.StartDate,
            EndDate = project.EndDate == default ? null : project.EndDate,
            InteractionForm = project.InteractionForm.ToString(),
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

    public static ProjectTopicDto From(Topic topic)
    {
        return new ProjectTopicDto
        {
            Id = topic.Id,
            Name = topic.Name,
            Context = topic.Context ?? string.Empty
        };
    }
}

public class QuestionDto
{
    public int Id { get; set; }
    public string ProjectSlug { get; set; }
    public string Text { get; set; }
    public bool IsRequired { get; set; }
    public string Type { get; set; }
    public IReadOnlyCollection<AnswerOptionDto> Options { get; set; } = Array.Empty<AnswerOptionDto>();

    public static QuestionDto From(Question question, string projectSlug, IReadOnlyCollection<AnswerOptionDto>? options = null)
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
            ProjectSlug = projectSlug,
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
