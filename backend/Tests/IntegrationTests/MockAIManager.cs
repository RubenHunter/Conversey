using Conversey.BL.Ai;
using Conversey.BL.Ai.DTOs;
using Conversey.BL.Domain.Common;
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

    public Task<ExtractKeyPhrasesResponse> ExtractKeyPhrases(
        string transcript,
        Language language,
        int maxPhrases,
        IReadOnlyList<string> existingPhrases = null,
        IReadOnlyList<string> rejectedPhrases = null)
    {
        if (string.IsNullOrWhiteSpace(transcript) || maxPhrases <= 0)
            return Task.FromResult<ExtractKeyPhrasesResponse>(new ExtractKeyPhrasesResponse([]));

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
        
        // Simple mock: combine transcript and bubbles, filtering out rejected phrases
        var filteredBubbles = bubbles.ToList();
        if (rejectedPhrases != null)
        {
            var rejectedSet = new HashSet<string>(rejectedPhrases, StringComparer.OrdinalIgnoreCase);
            filteredBubbles = filteredBubbles.Where(b => !rejectedSet.Contains(b)).ToList();
        }
        return Task.FromResult(transcript + " " + string.Join(", ", filteredBubbles));
    }

    public Task<string> CompletePlainTextAsync(
        string systemPrompt,
        string userPrompt,
        string? workspaceId = null,
        string? projectId = null,
        string? displayPromptName = null)
    {
        return Task.FromResult("[Mock] Plain text completion response.");
    }
}
