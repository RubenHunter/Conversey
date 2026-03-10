using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

public sealed class OpenTextAnswer : TextAnswer
{ 
    [StringLength(4000)]
    public string Value { get; set; }
}