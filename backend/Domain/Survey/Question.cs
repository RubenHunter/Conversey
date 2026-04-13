using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Survey;

public abstract class Question
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Text { get; set; }
    
    public bool Required { get; set; }
    public Image? Image { get; set; }
    
    [Required]
    public Project Project { get; set; }
}

public abstract class Question<TAnswer> : Question
    where TAnswer : Answer
{
    public IEnumerable<TAnswer> AnswerSubmissions { get; set; }
}

public class ChoiceQuestion<TChoice> : Question<Answer<TChoice>>
where TChoice : Choice<TChoice>
{
    public IList<TChoice> PossibleChoices { get; set; }
}

public class OpenQuestion : Question<Answer<string>>;

public class ScaleQuestion : Question<Answer<int>>
{
    [Required]
    public int LowerBound { get; set; }
    [Required]
    public int UpperBound { get; set; }
}
