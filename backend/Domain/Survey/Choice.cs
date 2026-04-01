using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Survey;

public abstract class Choice<TChoice>
    where TChoice : Choice<TChoice>
{
    [Required]
    public string Text { get; set; }

    [Required]
    public ChoiceQuestion<TChoice> Question { get; set; }
}

public class SingleChoice : Choice<SingleChoice>;

public class MultipleChoice : Choice<MultipleChoice>;