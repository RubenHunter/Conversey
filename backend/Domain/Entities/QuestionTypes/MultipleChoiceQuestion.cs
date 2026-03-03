namespace Conversey.BL.Domain.Entities.QuestionTypes;

public class MultipleChoiceQuestion : Question
{
    // To-Do 2 =< via validation
    private TextAnswer[] PossibleAnswers { get; set; }
}