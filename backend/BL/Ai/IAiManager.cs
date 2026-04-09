using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public interface IAiManager : IChatClient
{
    Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null);
    Task<ModerationDecision> ModerateContent(string content);
}