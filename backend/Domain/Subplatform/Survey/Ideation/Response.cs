using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Ideation;

public class Response
{
    [Required]
    public int Id { get; set; }

    public Subplatform.Survey.Ideation.Idea Idea { get; set; }
    public string text { get; set; }
    public DateTime createdAt { get; set; }
}