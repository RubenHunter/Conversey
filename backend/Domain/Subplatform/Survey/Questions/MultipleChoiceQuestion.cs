using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions;

public class MultipleChoiceQuestion : Question
{
    private IEnumerable<TextAnswer> PossibleAnswers { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();
        if (PossibleAnswers.Count() < 2)
            errors.Add(new ValidationResult("A Multiple choice question needs at least 2 possible answers", [nameof(PossibleAnswers)]));
        
        return errors;
    }
}