using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;

namespace Conversey.BL.Domain.Ideation;

public class Idea
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Content { get; set; }

    public string Summary { get; set; }

    public DateTime SubmissionDate { get; set; }
    public ModerationStatus Status { get; set; }
    public ModerationInfo ModerationInfo { get; set; }

    [Required]
    public Project Project { get; set; }

    [Required]
    public Topic Topic { get; set; }

    [Required]
    public Youth Youth { get; set; }

    public ICollection<IdeaReaction> Reactions { get; set; } = new List<IdeaReaction>();

    public ICollection<Response> Responses { get; set; } = new List<Response>();
}