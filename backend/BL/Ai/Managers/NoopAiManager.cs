using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Subplatform.Ai;

namespace Conversey.BL.Ai;

public sealed class NoopAiManager : IAiManager
{
    private readonly IModerationKeywordRepository _moderationKeywordRepository;

    private static readonly Dictionary<string, (int minWords, string placeholder)> NudgeThresholds = new()
    {
        ["Minimal"] = (0, "Can you say a bit more about this idea?"),
        ["Light"] = (4, "Can you make this idea a bit more specific for this topic?"),
        ["Medium"] = (8, "Can you elaborate on why this matters and who would benefit?"),
        ["Strong"] = (10, "What is the concrete impact of this idea and who exactly would it help?"),
        ["Deep"] = (14, "Can you provide specific details, evidence, or a concrete scenario that supports this idea?"),
    };

    public NoopAiManager(IModerationKeywordRepository moderationKeywordRepository)
    {
        _moderationKeywordRepository = moderationKeywordRepository;
    }

    public string GenerateAlternative(string content, ModerationDecision decision = null)
    {
        return "Please rephrase your message in a respectful way.";
    }

    public ModerationDecision ModerateContent(string content)
    {
        var keywordSet = _moderationKeywordRepository.GetKeywordSet();
        var unsafeTerm = keywordSet.FirstOrDefault(term => (content ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase));
        var isAllowed = unsafeTerm == null;

        return new ModerationDecision
        {
            IsAllowed = isAllowed,
            Categories = new ModerationInfo()
        };
    }

    public IdeaNudgeDecision AssessIdeaNudge(IdeaNudgeAssessmentRequest request)
    {
        var mode = (request.NudgingMode ?? "Medium").Trim();
        if (!NudgeThresholds.TryGetValue(mode, out var threshold))
        {
            threshold = NudgeThresholds["Medium"];
        }

        var wordCount = (request.IdeaText ?? string.Empty)
            .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        var isApproved = wordCount >= threshold.minWords;
        var question = isApproved ? null : threshold.placeholder;

        return new IdeaNudgeDecision
        {
            IsApproved = isApproved,
            Question = question
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
        var canonicalExisting = existingCategories
            .Select(c => (c ?? string.Empty).Trim())
            .Where(c => c.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (int index = 0; index < ideas.Count; index++)
        {
            var text = (ideas[index] ?? string.Empty).ToLowerInvariant();
            var categories = new List<string>();

            if (text.Contains("stress") || text.Contains("druk") || text.Contains("exam") || text.Contains("deadline"))
                categories.Add("Study Pressure");
            if (text.Contains("coach") || text.Contains("support") || text.Contains("psych") || text.Contains("help"))
                categories.Add("Support services");
            if (text.Contains("campus") || text.Contains("group") || text.Contains("peer") || text.Contains("connected"))
                categories.Add("Community & belonging");
            if (text.Contains("online") || text.Contains("digital") || text.Contains("hybrid"))
                categories.Add("Digital learning");

            if (categories.Count == 0)
            {
                if (canonicalExisting.Count > 0)
                {
                    categories.AddRange(canonicalExisting.Take(Math.Max(1, maxCategoriesPerIdea)));
                }
                else
                {
                    categories.Add("General ideas");
                }
            }

            var normalized = categories
                .Select(c => canonicalExisting.FirstOrDefault(existing => NormalizeCategoryKey(existing) == NormalizeCategoryKey(c)) ?? c)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, maxCategoriesPerIdea))
                .ToList();

            result[index] = normalized.AsReadOnly();
        }

        return result;
    }

    private static string NormalizeCategoryKey(string value)
    {
        return new string((value ?? string.Empty)
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }
}
