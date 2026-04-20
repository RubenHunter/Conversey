using System.ComponentModel.DataAnnotations;

namespace Conversey.REST.Models.Dto;

public class UpdateResponseAfterSafetyReviewDto
{
    [Required]
    public string Text { get; set; }

    [Required]
    public Guid YouthId { get; set; }

    public bool MarkForReview { get; set; }
}

