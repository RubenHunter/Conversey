using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Workspace.Project.Question.Answer;

public class TextAnswer
{
    [Required]
    public int Id { get; set; }
    public Youth Youth { get; set; }
}