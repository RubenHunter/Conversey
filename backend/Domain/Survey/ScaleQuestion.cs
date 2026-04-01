using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Survey;

public class ScaleQuestion : Question<IntegerAnswer>
{
    [Required]
    public int Lowerbound { get; set; }
    [Required]
    public int Upperbound { get; set; }
}