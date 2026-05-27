using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;

namespace Conversey.BL.Domain.Survey;

public abstract class Answer
{
    [Required]
    public int Id { get; set; }

    [Required]
    public Youth Youth { get; set; }
    
    [Required]
    public Question Question { get; set; }
}

public class Answer<TValueType> : Answer
{
    [Required]
    public TValueType Value { get; set; }
}

public class SingleChoiceAnswer : Answer<Choice>;

public class MultipleChoiceAnswer : Answer<IEnumerable<Choice>>;

