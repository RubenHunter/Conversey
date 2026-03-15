using System.ComponentModel.DataAnnotations;

namespace Conversey.REST.Models.Dto;

public class UpdateIdeaAfterSafetyReviewDto
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string YouthToken { get; set; } = string.Empty;

    public bool MarkForReview { get; set; }
}

