using Conversey.BL.Ai;
using Conversey.BL.Domain.Ideation;

namespace Tests.IntegrationTests;

public class MockAiManager : IAiManager
{
    public string GenerateAlternative(string content, ModerationDecision decision = null)
    {
        return $"[Mock] Alternative text for: {content}";
    }

    public ModerationDecision ModerateContent(string content)
    {
        return new ModerationDecision
        {
            IsAllowed = true,
            Categories = new ModerationInfo()
        };
    }

    public IdeaNudgeDecision AssessIdeaNudge(IdeaNudgeAssessmentRequest request)
    {
        return new IdeaNudgeDecision
        {
            IsApproved = true
        };
    }

    public IEnumerable<int> RankIdeasByRelation(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
    {
        if (candidateIdeas.Count == 0 || limit <= 0)
        {
            return Array.Empty<int>();
        }

        var ordered = Enumerable.Range(0, candidateIdeas.Count);
        if (preferDifferent)
        {
            ordered = ordered.Reverse();
        }

        return ordered.Take(limit);
    }

    public IReadOnlyDictionary<int, IReadOnlyList<string>> CategorizeIdeas(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
    {
        var result = new Dictionary<int, IReadOnlyList<string>>();
        for (int index = 0; index < ideas.Count; index++)
        {
            result[index] = new[] { existingCategories.FirstOrDefault() ?? "General ideas" };
        }

        return result;
    }
}
