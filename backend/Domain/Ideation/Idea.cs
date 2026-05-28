#nullable enable
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
    public bool QualityNudgeBypassed { get; set; }
    public string? RejectionReason { get; set; }
    public bool MarkedForReview => Status == ModerationStatus.Pending;

    public string[] SemanticCategories { get; set; } = Array.Empty<string>();

    [Required]
    public Project Project { get; set; }

    [Required]
    public Topic Topic { get; set; }

    [Required]
    public Youth Youth { get; set; }

    public IEnumerable<IdeaReaction> Reactions { get; set; } = new List<IdeaReaction>();

    public IEnumerable<IdeaResponse> Responses { get; set; } = new List<IdeaResponse>();
}