using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions;

public class OpenQuestion : Question
{
    // To-Do 2 =< via validation
    private TextAnswer PossibleAnswers { get; set; }
}