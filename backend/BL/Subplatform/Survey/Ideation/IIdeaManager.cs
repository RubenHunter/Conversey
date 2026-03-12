using Conversey.BL.Ai;
using Conversey.BL.Domain.Subplatform.Survey;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public interface IIdeaManager
{
    Task<string> ReviewIdeaAsync(string contentText);
    Task<ModerationDecision> IsIdeaAllowedAsync(string ideaDescription);
    Task<string> GenerateAIAlternativeAsync(string prompt);
}