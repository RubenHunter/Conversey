using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;

namespace Conversey.BL.Domain.Ideation;

public class Response
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Text { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public ModerationStatus Status { get; set; }
    
    public ModerationInfo ModerationInfo { get; set; }
    
    [Required]
    public Idea Idea { get; set; }

    [Required]
    public Youth Youth { get; set; }

    public IEnumerable<ResponseReaction> Reactions { get; set; }
}