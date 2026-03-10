using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

public sealed class ClosedTextAnswer : TextAnswer
{
    [StringLength(100)]
    public string Value { get; set; }
}