using Conversey.BL.Domain.Workspace.Project.Question.Answer;

namespace Conversey.BL.Domain.Workspace.Project.Question;

public class OpenQuestion : Question
{
    // To-Do 2 =< via validation
    private TextAnswer PossibleAnswers { get; set; }
}