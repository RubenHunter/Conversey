using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ai;

public interface IAiManager
{
    Task<string> GenerateAlternativeAsync(string content, ModerationDecision decision = null);
    Task<ModerationDecision> ModerateContentAsync(string content);
    Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(IdeaNudgeAssessmentRequest request);
    Task<IEnumerable<int>> RankIdeasByRelationAsync(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit);
    Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeasAsync(
        IReadOnlyList<string> ideas,
        IReadOnlyList<string> existingCategories,
        int maxCategoriesPerIdea);
}
