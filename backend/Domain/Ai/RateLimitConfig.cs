using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Ai;

public class RateLimitConfig
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string PolicyName { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int PermitLimit { get; set; } = 30;

    [Range(1, 3600)]
    public int WindowSeconds { get; set; } = 60;

    [Range(0, 100)]
    public int QueueLimit { get; set; } = 0;

    [Required]
    [MaxLength(50)]
    public string PartitionType { get; set; } = "global";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
