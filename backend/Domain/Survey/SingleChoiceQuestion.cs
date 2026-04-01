using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Survey;

public class SingleChoiceQuestion : Question<TextAnswer>, IValidatableObject
{
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PossibleAnswers.Count() < 2)
        {
            yield return new ValidationResult(
                "A single choice question needs at least 2 options.",
                [nameof(PossibleAnswers)]);
        }
    }
}