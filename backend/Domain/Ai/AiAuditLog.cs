using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Ai;

public class AiAuditLog
{
    public int Id { get; set; }

    [Required]
    public string ModelName { get; set; } = string.Empty;

    [Required]
    public string ModelType { get; set; } = string.Empty;

    public int InputTokens { get; set; }

    public int OutputTokens { get; set; }

    public decimal Cost { get; set; }

    [MaxLength(50)]
    public string ProviderName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PromptName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public TimeSpan Duration { get; set; }

    public DateTime CreatedAt { get; set; }
}