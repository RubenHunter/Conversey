using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Ai;

public class AiPrompt
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string SystemPrompt { get; set; } = string.Empty;

    public string UserPromptTemplate { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
