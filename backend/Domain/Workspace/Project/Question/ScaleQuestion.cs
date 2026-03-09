using Conversey.BL.Domain.Workspace.Project.Question.Answer;

namespace Conversey.BL.Domain.Workspace.Project.Question;

public class ScaleQuestion : Question
{
    public int Lowerbound { get; set; }
    public int Upperbound { get; set; }
    public IntegerAnswer IntegerAnswer { get; set; }
}