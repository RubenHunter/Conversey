using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public interface IAiManager : IChatClient
{
    Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null);
    Task<ModerationDecision> ModerateContent(string content);
    Task<IdeaNudgeDecision> AssessIdeaNudge(IdeaNudgeAssessmentRequest request);
    Task<IEnumerable<int>> RankIdeasByRelation(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit);
    Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeas(
        IReadOnlyList<string> ideas,
        IReadOnlyList<string> existingCategories,
        int maxCategoriesPerIdea);
}