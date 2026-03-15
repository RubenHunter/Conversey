using System.ComponentModel.DataAnnotations;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions;

public class MultipleChoiceQuestion : Question, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Options.Count < 2)
        {
            yield return new ValidationResult(
                "A multiple choice question needs at least 2 options.",
                [nameof(Options)]);
        }
    }
}