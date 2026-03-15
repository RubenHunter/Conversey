using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

public abstract class Answer
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string YouthToken { get; set; } = string.Empty;

    public int QuestionId { get; set; }

    [Required]
    public Youth Youth { get; set; }

    [Required]
    public Question Question { get; set; }
}

