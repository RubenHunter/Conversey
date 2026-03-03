using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Entities.Identity;

namespace Conversey.BL.Domain.Entities.Question.AnswerTypes;

public class IntegerAnswer
{
    [Required]
    public int Id { get; set; }
    public int Value { get; set; }
    public Youth Youth { get; set; }
}