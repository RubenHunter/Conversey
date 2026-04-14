using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;

namespace Conversey.BL.Domain.Ideation;

public class Idea
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(4000)]
    public string Content { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Summary { get; set; } = string.Empty;

    public DateTime SubmissionDate { get; set; }
    public ModerationStatus Status { get; set; }
    public ModerationInfo ModerationInfo { get; set; }

    [Required]
    public Project Project { get; set; }

    [Required]
    public Topic Topic { get; set; }

    [Required]
    public Youth Youth { get; set; }

    public IEnumerable<IdeaReaction> Reactions { get; set; } = new List<IdeaReaction>();

    public IEnumerable<Response> Responses { get; set; } = new List<Response>();
}