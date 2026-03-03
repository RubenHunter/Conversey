namespace Conversey.BL.Domain.Entities.QuestionTypes;

public class ScaleQuestion : Question
{
    public int Lowerbound { get; set; }
    public int Upperbound { get; set; }
    
}