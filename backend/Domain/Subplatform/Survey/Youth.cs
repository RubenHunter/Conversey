using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey;

public class Youth
{
    [Key]
    [Required]
    public string Token { get; set; }

    public Subplatform.Survey.Project Project { get; set; }
    
    public string? Email { get; set; }
}