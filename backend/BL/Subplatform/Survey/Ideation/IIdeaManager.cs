using Conversey.BL.Ai;
using Conversey.BL.Domain.Subplatform.Survey;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public interface IIdeaManager
{
    string SubmitIdea(string contentText, bool forceSubmit);
    ModerationDecision IsIdeaAllowed(string ideaDescription);
    string GenerateAiAlternative(string prompt);
}