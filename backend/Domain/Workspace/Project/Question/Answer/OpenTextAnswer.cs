using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace.Project.Question.Answer;

public sealed class OpenTextAnswer : TextAnswer
{ 
    [StringLength(4000)]
    public string Value { get; set; }
}