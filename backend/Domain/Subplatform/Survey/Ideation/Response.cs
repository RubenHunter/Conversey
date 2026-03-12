using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Ideation;

public class Response
{
    [Required]
    public int Id { get; set; }

    public Idea Idea { get; set; }
    public string text { get; set; }
    public DateTime createdAt { get; set; } = DateTime.Now;
    public IdeaStatus Status { get; set; }
    public ModerationInfo? ModerationInfo { get; set; }
}