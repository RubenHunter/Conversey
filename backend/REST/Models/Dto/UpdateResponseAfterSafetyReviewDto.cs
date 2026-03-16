using System.ComponentModel.DataAnnotations;

namespace Conversey.REST.Models.Dto;

public class UpdateResponseAfterSafetyReviewDto
{
    [Required]
    public string Text { get; set; } = string.Empty;

    [Required]
    public string YouthToken { get; set; } = string.Empty;

    public bool MarkForReview { get; set; }
}

