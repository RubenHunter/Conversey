using Conversey.BL.Ai;
using Conversey.BL.Domain.Ideation;

namespace Tests.IntegrationTests;

public class MockAiManager : IAiManager
{
    public Task<string> GenerateAlternativeAsync(string content, ModerationDecision decision = null, string? workspaceId = null, string? projectId = null)
    {
        return Task.FromResult($"[Mock] Alternative text for: {content}");
    }

    public Task<ModerationDecision> ModerateContentAsync(string content, string? workspaceId = null, string? projectId = null)
    {
        return Task.FromResult(new ModerationDecision
        {
            IsAllowed = true,
            Categories = new ModerationInfo()
        });
    }

    public Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(IdeaNudgeAssessmentRequest request, string? workspaceId = null, string? projectId = null)
    {
        return Task.FromResult(new IdeaNudgeDecision
        {
            IsApproved = true
        });
    }

    public Task<IEnumerable<int>> RankIdeasByRelationAsync(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit, string? workspaceId = null, string? projectId = null)
    {
        if (candidateIdeas.Count == 0 || limit <= 0)
        {
            return Task.FromResult<IEnumerable<int>>(Array.Empty<int>());
        }

        var ordered = Enumerable.Range(0, candidateIdeas.Count);
        if (preferDifferent)
        {
            ordered = ordered.Reverse();
        }

        return Task.FromResult<IEnumerable<int>>(ordered.Take(limit).ToList());
    }

    public Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeasAsync(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea, string? workspaceId = null, string? projectId = null)
    {
        var result = new Dictionary<int, IReadOnlyList<string>>();
        for (int index = 0; index < ideas.Count; index++)
        {
            result[index] = new[] { existingCategories.FirstOrDefault() ?? "General ideas" };
        }

        return Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<string>>>(result);
    }
}
