using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Ai;

public class AiAuditLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ModelName { get; set; } = string.Empty;

    [Required]
    public string ModelType { get; set; } = string.Empty;

    [Required]
    public int InputTokens { get; set; }

    [Required]
    public int OutputTokens { get; set; }

    [Required]
    public decimal Cost { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public TimeSpan Duration { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }
}