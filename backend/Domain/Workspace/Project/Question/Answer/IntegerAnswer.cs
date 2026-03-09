using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace.Project.Question.Answer;

public class IntegerAnswer
{
    [Required]
    public int Id { get; set; }
    public int Value { get; set; }
    public Youth Youth { get; set; }
}