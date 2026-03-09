using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Entities.Idea___interaction;

public class Idea
{
    [Required]
    public int Id { get; set; }

    public string content { get; set; }
    public string Summary { get; set; }
    public DateTime submissionDate { get; set; }
    public IdeaStatus Status { get; set; }

    public Project.Project Project { get; set; }
    public IEnumerable<Response> Responses { get; set; }
}