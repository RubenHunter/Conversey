using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Ideation;

public class Response
{
    [Required]
    public int Id { get; set; }

    [Required]
    public Idea Idea { get; set; }

    [Required]
    [StringLength(4000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    [Required]
    public Youth Youth { get; set; }

    public ICollection<ResponseReaction> Reactions { get; set; } = new List<ResponseReaction>();
    
    public IdeaStatus Status { get; set; }
    public ModerationInfo ModerationInfo { get; set; }
}