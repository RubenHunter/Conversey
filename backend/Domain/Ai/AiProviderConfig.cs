using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Ai;

public class AiProviderConfig
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ProviderName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    [MaxLength(100)]
    public string CompletionsModel { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ModerationModel { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SttModel { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TtsModel { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ApiVersion { get; set; } = string.Empty;

    [Range(0, 2)]
    public decimal Temperature { get; set; } = 1.0m;

    public bool IsEnabled { get; set; }

    public DateTime? ApiKeyExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDefault { get; set; }
}
