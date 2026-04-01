using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;

namespace Conversey.BL.Domain.Survey;

public class AnsweredAnswer
{
    [Required]
    public Youth Youth { get; set; }
    [Required]
    public Answer Answer { get; set; }
}