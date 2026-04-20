using System.ComponentModel.DataAnnotations;

namespace Conversey.UI_MVC.Models.Dto;

public class UpdateIdeaAfterSafetyReviewDto
{

    [Required]
    public string Content { get; set; }


    public bool MarkForReview { get; set; }
}

