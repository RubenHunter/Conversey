using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

public class IntegerAnswer
{
    [Required]
    public int Id { get; set; }
    public int Value { get; set; }
    public Youth Youth { get; set; }
    public Question Question { get; set; }
}

