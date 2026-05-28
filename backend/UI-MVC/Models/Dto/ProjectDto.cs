using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Survey;

namespace Conversey.UI_MVC.Models.Dto;

public class ProjectDto
{
    public Slug Id { get; set; }
    public Slug OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public string? OrganizationLogo { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string InteractionForm { get; set; }
    public int NudgingStrength { get; set; }
    public ProjectTopicDto Topic { get; set; }
    public IEnumerable<ProjectTopicDto> Topics { get; set; } = Array.Empty<ProjectTopicDto>();
    public ProjectThemeDto Theme { get; set; }

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
            OrganizationLogo = project.Workspace.ImageUrl,
            Name = project.Name,
            Description = project.Description ?? string.Empty,
            ImageUrl = project.ImageUrl ?? string.Empty,
            Status = project.Status.ToString(),
            StartDate = project.StartDate == default ? null : project.StartDate,
            EndDate = project.EndDate == default ? null : project.EndDate,
            InteractionForm = project.InteractionForm.ToString(),
            NudgingStrength = project.NudgingStrength,
            Topic = firstTopic is null ? null : ProjectTopicDto.From(firstTopic),
            Topics = topics,
            Theme = ProjectThemeDto.From(project.Theme)
        };
    }
}

public class ProjectThemeDto
{
    public string Primary { get; set; }
    public string Secondary { get; set; }
    public string Accent { get; set; }
    public string Preset { get; set; }
    public string Font { get; set; }

    public static ProjectThemeDto From(ProjectTheme theme)
    {
        var t = theme ?? ProjectTheme.Default;
        return new ProjectThemeDto
        {
            Primary = t.Primary,
            Secondary = t.Secondary,
            Accent = t.Accent,
            Preset = t.Preset,
            Font = t.Font
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
    public string Text { get; set; }
    public bool Required { get; set; }
    public string Type { get; set; }
    public int LowerBound { get; set; }
    public int UpperBound { get; set; }
    public IEnumerable<ChoiceDto> PossibleAnswers { get; set; }

    public static QuestionDto From(Question question) 
    {
        var dto = new QuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            Required = question.Required,
            Type = question switch
            {
                SingleChoiceQuestion => "SingleChoice",
                MultipleChoiceQuestion => "MultipleChoice",
                ScaleQuestion => "Scale",
                OpenQuestion => "Open",
                _ => throw new ArgumentException($"Unexpected question type: {question.GetType()}")
            }
        };

        switch (question)
        {
            case SingleChoiceQuestion choiceQuestion:
                dto.PossibleAnswers = choiceQuestion.PossibleChoices.Select(ChoiceDto.From);
                break;
            case MultipleChoiceQuestion multipleChoiceQuestion:
                dto.PossibleAnswers = multipleChoiceQuestion.PossibleChoices.Select(ChoiceDto.From);
                break;
            case ScaleQuestion scaleQuestion:
                dto.LowerBound = scaleQuestion.LowerBound;
                dto.UpperBound = scaleQuestion.UpperBound;
                break;
        }
        return dto;
    }
}

public class ChoiceDto
{
    public int Id { get; set; }
    public string Text { get; set; }

    public static ChoiceDto From(Choice choice)
    {
        return new ChoiceDto
        {
            Id = choice.Id,
            Text = choice.Text
        };
    }
}
