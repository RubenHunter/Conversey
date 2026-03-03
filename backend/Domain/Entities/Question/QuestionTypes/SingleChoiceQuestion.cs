using Conversey.BL.Domain.Entities.Question.AnswerTypes;

namespace Conversey.BL.Domain.Entities.Question.QuestionTypes;

public class SingleChoiceQuestion : Question
{
    // To-Do 2 =< via validation
    private TextAnswer[] PossibleAnswer { get; set; }
}