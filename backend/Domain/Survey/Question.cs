using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Domain.Survey;

public abstract class Question
{
    [Required]
    public int Id { get; set; }

    public string Text { get; set; }
    public bool Required { get; set; }
    public Image? Image { get; set; }

    public Project Project { get; set; }
    
}

public class Question<TAnswerType> : Question
where TAnswerType : Answer
{
    public IOrderedEnumerable<TAnswerType> PossibleAnswers { get; set; }
}
