using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;

namespace Conversey.BL.Domain.Survey;

public class SurveySubmission
{
    [Required]
    public IEnumerable<Answer> Answers { get; set; }
    [Required]
    public Youth Youth { get; set; }
    [Required]
    public Project Project { get; set; }
}