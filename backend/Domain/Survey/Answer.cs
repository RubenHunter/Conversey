using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Survey;

public abstract class Answer
{
    [Required]
    public int Id { get; set; }
}

public class Answer<TValueType> : Answer
{
    [Required]
    public TValueType Value { get; set; }
    
    [Required]
    public Question<Answer<TValueType>> Question { get; set; }
}

