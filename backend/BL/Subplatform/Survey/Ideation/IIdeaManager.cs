using Conversey.BL.Ai;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public interface IIdeaManager
{
    Task<ModerationDecision> IsIdeaAllowedAsync(string ideaDescription);
    Task<string> GenerateAISuggestionAsync(string prompt);
}