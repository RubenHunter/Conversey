using System.ComponentModel.DataAnnotations;

namespace Conversey.UI_MVC.Models.Dto;

public class IdeaNudgeTurnDto
{
    [Required]
    public string Question { get; set; }

    [Required]
    public string Answer { get; set; }
}

public class IdeaNudgeRequestDto
{
    [Required]
    public string IdeaText { get; set; }

    [Required]
    public string ProjectTitle { get; set; }

    public string ProjectDescription { get; set; }

    [Required]
    public string TopicTitle { get; set; }

    public string TopicPrompt { get; set; }

    public IReadOnlyList<IdeaNudgeTurnDto> Conversation { get; set; } = Array.Empty<IdeaNudgeTurnDto>();
}

public class IdeaNudgeResponseDto
{
    public bool IsApproved { get; set; }
    public string Question { get; set; }
}

