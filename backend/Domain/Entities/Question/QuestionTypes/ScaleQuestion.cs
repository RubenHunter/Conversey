using Conversey.BL.Domain.Entities.Question.AnswerTypes;

namespace Conversey.BL.Domain.Entities.Question.QuestionTypes;

public class ScaleQuestion : Question
{
    public int Lowerbound { get; set; }
    public int Upperbound { get; set; }
    public IntegerAnswer IntegerAnswer { get; set; }
}