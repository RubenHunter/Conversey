using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Entities.Question.AnswerTypes;

public sealed class ClosedTextAnswer : TextAnswer
{
    [StringLength(100)]
    public string Value { get; set; }
}