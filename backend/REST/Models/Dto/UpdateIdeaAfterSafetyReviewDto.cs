using System.ComponentModel.DataAnnotations;

namespace Conversey.REST.Models.Dto;

public class UpdateIdeaAfterSafetyReviewDto
{

    [Required]
    public string Content { get; set; }

    [Required]
    public Guid YouthId { get; set; }

    public bool MarkForReview { get; set; }
}

