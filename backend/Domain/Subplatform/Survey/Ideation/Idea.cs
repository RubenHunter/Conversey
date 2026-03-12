using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Ideation;

public class Idea
{
    [Required]
    public int Id { get; set; }

    public string content { get; set; }
    public string Summary { get; set; }
    public DateTime submissionDate { get; set; } = DateTime.Now;
    public IdeaStatus Status { get; set; }
    public ModerationInfo? ModerationInfo { get; set; }

    public Project Project { get; set; }
    public IEnumerable<Response> Responses { get; set; }
}