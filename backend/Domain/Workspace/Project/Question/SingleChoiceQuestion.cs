using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Workspace.Project.Question.Answer;

namespace Conversey.BL.Domain.Workspace.Project.Question;

public class SingleChoiceQuestion : Question
{
    private IEnumerable<TextAnswer> PossibleAnswers { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();
        if (PossibleAnswers.Count() < 2)
            errors.Add(new ValidationResult("A Single choice question needs at least 2 possible answers", [nameof(PossibleAnswers)]));
        
        return errors;
    }
}