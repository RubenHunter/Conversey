using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Questions;

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
    public ProjectTopicDto? Topic { get; set; }

    public static ProjectDto From(Project project)
    {
        Topic? topic = project.Topic?.FirstOrDefault();

        return new ProjectDto
        {
            Id = project.Id,
            Slug = project.Slug.Text,
            OrganizationSlug = project.Workspace.Slug.Text,
            OrganizationName = project.Workspace.Name,
            Title = project.Title,
            Description = project.Description ?? string.Empty,
            ImageUrl = project.ImageUrl ?? string.Empty,
            Status = project.Status.ToString(),
            StartDate = project.StartDate == default ? null : project.StartDate,
            EndDate = project.EndDate == default ? null : project.EndDate,
            InteractionForm = project.InteractionForm.ToString(),
            Topic = topic is null ? null : ProjectTopicDto.From(topic)
        };
    }
}

public class ProjectTopicDto
{
    public string Name { get; set; }
    public string Context { get; set; }

    public static ProjectTopicDto From(Topic topic)
    {
        return new ProjectTopicDto
        {
            Name = topic.Name,
            Context = topic.Context ?? string.Empty
        };
    }
}

public class QuestionDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Text { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public string Type { get; set; }
    public IReadOnlyCollection<AnswerOptionDto> Options { get; set; } = Array.Empty<AnswerOptionDto>();

    public static QuestionDto From(Question question, int projectId)
    {
        return new QuestionDto
        {
            Id = question.Id,
            ProjectId = projectId,
            Text = question.Text,
            Order = question.Order,
            IsRequired = question.IsRequired,
            Type = question switch
            {
                SingleChoiceQuestion => "SingleChoice",
                MultipleChoiceQuestion => "MultipleChoice",
                ScaleQuestion => "Scale",
                _ => "OpenText"
            },
            Options = question.Options
                .OrderBy(option => option.Order)
                .ThenBy(option => option.Id)
                .Select(option => AnswerOptionDto.From(option, question.Id))
                .ToList()
                .AsReadOnly()
        };
    }
}

public class AnswerOptionDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; }

    public static AnswerOptionDto From(QuestionOption option, int questionId)
    {
        return new AnswerOptionDto
        {
            Id = option.Id,
            QuestionId = questionId,
            Text = option.Text
        };
    }
}
