using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Survey;

public abstract class Question
{
    public int Id { get; set; }

    [Required]
    public string Text { get; set; }
    
    public bool Required { get; set; }
    
    [NotMapped]
    public Image? Image { get; set; }
    
    [Required]
    public Project Project { get; set; }
}

public abstract class ChoiceQuestion : Question
{
    public IEnumerable<Choice> PossibleChoices { get; set; }
}

public class SingleChoiceQuestion : ChoiceQuestion
{
    public IEnumerable<SingleChoiceAnswer> AnsweredAnswers { get; set; }
}

public class MultipleChoiceQuestion : ChoiceQuestion
{
    public IEnumerable<MultipleChoiceAnswer> AnsweredAnswers { get; set; }
}

public class OpenQuestion : Question
{
    public IEnumerable<Answer<string>> AnsweredAnswers { get; set; }
}

public class ScaleQuestion : Question
{
    [Required]
    public int LowerBound { get; set; }
    [Required]
    public int UpperBound { get; set; }
    
    public IEnumerable<Answer<int>> AnsweredAnswers { get; set; }
}
