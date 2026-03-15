namespace Conversey.BL.Ai;

public interface IAiManager
{
    string GenerateAiAlternative(string prompt);
    ModerationDecision ModerateContent(string ideaDescription);
}