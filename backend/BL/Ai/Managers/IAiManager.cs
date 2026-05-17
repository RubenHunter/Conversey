using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ai;

public interface IAiManager
{
    Task<string> GenerateAlternativeAsync(string content, ModerationDecision decision = null, string? workspaceId = null, string? projectId = null);
    Task<ModerationDecision> ModerateContentAsync(string content, string? workspaceId = null, string? projectId = null);
    Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(IdeaNudgeAssessmentRequest request, string? workspaceId = null, string? projectId = null);
    Task<IEnumerable<int>> RankIdeasByRelationAsync(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit, string? workspaceId = null, string? projectId = null);
    Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeasAsync(
        IReadOnlyList<string> ideas,
        IReadOnlyList<string> existingCategories,
        int maxCategoriesPerIdea,
        string? workspaceId = null,
        string? projectId = null);
}
