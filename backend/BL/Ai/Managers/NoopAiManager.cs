using Conversey.BL.Ai.DTOs;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Subplatform.Ai;

using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ai;

namespace Conversey.BL.Ai;

public sealed class NoopAiManager : IAiManager
{
    private readonly IModerationKeywordRepository _moderationKeywordRepository;

    private static readonly Dictionary<NudgingMode, (int minWords, string placeholder)> NudgeThresholds = new()
    {
        [NudgingMode.Minimal] = (0, "Can you say a bit more about this idea?"),
        [NudgingMode.Light] = (4, "Can you make this idea a bit more specific for this topic?"),
        [NudgingMode.Medium] = (8, "Can you elaborate on why this matters and who would benefit?"),
        [NudgingMode.Strong] = (10, "What is the concrete impact of this idea and who exactly would it help?"),
        [NudgingMode.Deep] = (14, "Can you provide specific details, evidence, or a concrete scenario that supports this idea?"),
    };

    public NoopAiManager(IModerationKeywordRepository moderationKeywordRepository)
    {
        _moderationKeywordRepository = moderationKeywordRepository;
    }

    public Task<string> GenerateAlternativeAsync(string content, ModerationDecision decision = null, string? workspaceId = null, string? projectId = null)
    {
        return Task.FromResult("Please rephrase your message in a respectful way.");
    }

    public Task<ModerationDecision> ModerateContentAsync(string content, string? workspaceId = null, string? projectId = null)
    {
        var keywordSet = _moderationKeywordRepository.GetKeywordSet();
        var unsafeTerm = keywordSet.FirstOrDefault(term => (content ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase));
        var isAllowed = unsafeTerm == null;

        return Task.FromResult(new ModerationDecision
        {
            IsAllowed = isAllowed,
            Categories = new ModerationInfo()
        });
    }

    public Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(IdeaNudgeAssessmentRequest request, string? workspaceId = null, string? projectId = null)
    {
        if (!NudgeThresholds.TryGetValue(request.NudgingMode, out var threshold))
        {
            threshold = NudgeThresholds[NudgingMode.Medium];
        }

        var wordCount = (request.IdeaText ?? string.Empty)
            .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Length;

        var isApproved = wordCount >= threshold.minWords;
        var question = isApproved ? null : threshold.placeholder;

        return Task.FromResult(new IdeaNudgeDecision
        {
            IsApproved = isApproved,
            Question = question
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

        return Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<string>>>(result);
    }

    public Task<ExtractKeyPhrasesResponse> ExtractKeyPhrases(
        string transcript,
        Language language,
        int maxPhrases,
        IReadOnlyList<string> existingPhrases = null,
        IReadOnlyList<string> rejectedPhrases = null)
    {
        if (string.IsNullOrWhiteSpace(transcript) || maxPhrases <= 0)
            return Task.FromResult(new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>()));

        var rejected = rejectedPhrases?.Select(p => p.Trim().ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();
        var existing = existingPhrases?.Select(p => p.Trim().ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();

        var sentences = transcript
            .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0 && !rejected.Contains(s.ToLowerInvariant()) && !existing.Contains(s.ToLowerInvariant()))
            .Take(maxPhrases)
            .ToList()
            .AsReadOnly();

        return Task.FromResult(new ExtractKeyPhrasesResponse(sentences));
    }

    public Task<string> GenerateTextFromBubbles(
        string transcript,
        IReadOnlyList<string> bubbles,
        Language language,
        IReadOnlyList<string> rejectedPhrases = null)
    {
        if (string.IsNullOrWhiteSpace(transcript) || bubbles == null || bubbles.Count == 0)
            return Task.FromResult(string.Empty);

        var filteredBubbles = bubbles.ToList();
        if (rejectedPhrases != null)
        {
            var rejectedSet = new HashSet<string>(rejectedPhrases, StringComparer.OrdinalIgnoreCase);
            filteredBubbles = filteredBubbles.Where(b => !rejectedSet.Contains(b)).ToList();
        }

        return Task.FromResult(transcript + " " + string.Join(", ", filteredBubbles));
    }

    private static string NormalizeCategoryKey(string value)
    {
        return new string((value ?? string.Empty)
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }
}
