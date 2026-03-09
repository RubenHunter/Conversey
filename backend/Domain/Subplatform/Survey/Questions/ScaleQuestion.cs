using Conversey.BL.Domain.Subplatform.Survey.Questions.Answers;

namespace Conversey.BL.Domain.Subplatform.Survey.Questions;

public class ScaleQuestion : Subplatform.Survey.Questions.Question
{
    public int Lowerbound { get; set; }
    public int Upperbound { get; set; }
    public IntegerAnswer IntegerAnswer { get; set; }
}