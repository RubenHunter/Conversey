using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace.Project.Idea;

public class Response
{
    [Required]
    public int Id { get; set; }

    public Idea Idea { get; set; }
    public string text { get; set; }
    public DateTime createdAt { get; set; }
}