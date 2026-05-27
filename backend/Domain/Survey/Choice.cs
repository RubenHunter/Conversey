using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Survey;

public class Choice
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Text { get; set; }
    
    public ChoiceQuestion Question { get; set; }
}