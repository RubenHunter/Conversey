using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Ideation;

public class Idea
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Content { get; set; }
    public string Summary { get; set; }
    public DateTime SubmissionDate { get; set; }
    public IdeaStatus Status { get; set; }

    [Required]
    public Project Project { get; set; }
    
    [Required]
    public Topic Topic { get; set; }
    public IEnumerable<Response> Responses { get; set; }
}