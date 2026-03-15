using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Ideation;

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
    public IdeaStatus Status { get; set; }

    public int ProjectId { get; set; }

    [Required]
    public Project Project { get; set; }

    public int TopicId { get; set; }

    [Required]
    public Topic Topic { get; set; }

    [Required]
    public string YouthToken { get; set; } = string.Empty;

    [Required]
    public Youth Youth { get; set; }

    public ICollection<Response> Responses { get; set; } = new List<Response>();
}