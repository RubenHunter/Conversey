using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;

namespace Conversey.BL.Domain.Ideation;

public abstract class Reaction
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Emoji { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public Youth Youth { get; set; }
}
public sealed class IdeaReaction : Reaction
{
    [Required]
    public Idea Idea { get; set; }
}

public sealed class ResponseReaction : Reaction
{
    [Required]
    public Response Response { get; set; }
}