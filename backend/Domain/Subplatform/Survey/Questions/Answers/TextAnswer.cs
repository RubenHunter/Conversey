using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

public class TextAnswer
{
    [Required]
    public int Id { get; set; }
    public Youth Youth { get; set; }
}