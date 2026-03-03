using Conversey.BL.Domain.Entities.AnswerTypes;

namespace Conversey.BL.Domain.Entities.QuestionTypes;

public class OpenQuestion : Question
{
    // To-Do 2 =< via validation
    private TextAnswer[]? PossibleAnswers { get; set; }
}