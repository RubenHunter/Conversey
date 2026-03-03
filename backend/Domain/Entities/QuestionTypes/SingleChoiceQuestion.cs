namespace Conversey.BL.Domain.Entities.QuestionTypes;

public class SingleChoiceQuestion : Question
{
    // To-Do 2 =< via validation
    private TextAnswer[] PossibleAnswer { get; set; }
}