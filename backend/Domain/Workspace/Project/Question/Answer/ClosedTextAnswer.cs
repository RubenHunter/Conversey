using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace.Project.Question.Answer;

public sealed class ClosedTextAnswer : TextAnswer
{
    [StringLength(100)]
    public string Value { get; set; }
}