using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Ideation;

public class ResponseReaction
{
    [Required]
    public int Id { get; set; }

    public int ResponseId { get; set; }

    [Required]
    public Response Response { get; set; }

    [Required]
    [StringLength(32)]
    public string Emoji { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    [Required]
    public string YouthToken { get; set; } = string.Empty;

    [Required]
    public Youth Youth { get; set; }
}

