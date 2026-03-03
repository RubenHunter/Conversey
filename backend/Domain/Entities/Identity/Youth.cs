using Conversey.BL.Domain.Entities.AnswerTypes;

namespace Conversey.BL.Domain.Entities.Identity;

public class Youth
{
    public string Token { get; set; }
    public TextAnswer[] TextAnswers { get; set; }
}