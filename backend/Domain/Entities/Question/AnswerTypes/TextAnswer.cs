using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Entities.Identity;

namespace Conversey.BL.Domain.Entities.Question.AnswerTypes;

public class TextAnswer
{
    [Required]
    public int Id { get; set; }
    
    public string Value { get; set; }
    public Youth Youth { get; set; }
}