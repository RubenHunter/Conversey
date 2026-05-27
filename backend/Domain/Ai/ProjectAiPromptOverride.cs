using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Ai;

public class ProjectAiPromptOverride
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProjectId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PromptName { get; set; } = string.Empty;

    public string UserPromptTemplate { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
