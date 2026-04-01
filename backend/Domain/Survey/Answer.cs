using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Survey;

public abstract class Answer
{
    [Required]
    public int Id { get; set; }

    [Required]
    public Question Question { get; set; }
}

public class IntegerAnswer : Answer
{
    public int Value { get; set; }
}

public sealed class TextAnswer : Answer
{ 
    public string Value { get; set; }
}

