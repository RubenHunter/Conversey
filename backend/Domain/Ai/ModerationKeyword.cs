using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Ai;

public class ModerationKeyword
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Keyword { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
