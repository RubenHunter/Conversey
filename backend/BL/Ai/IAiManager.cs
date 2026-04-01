using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public interface IAiManager : IChatClient
{
    Task<string> GenerateAiAlternative(string prompt, AiModel model, ModerationDecision decision = null);
    Task<ModerationDecision> ModerateContent(string content, AiModel model);
}

public class AiModel
{
    public string Name { get; set; }
    public string Type { get; set; } // "Moderation", "Chat", etc.
}