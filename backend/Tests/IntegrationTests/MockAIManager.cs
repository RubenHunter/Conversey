using Conversey.BL.Ai;
using Conversey.BL.Domain.Ideation;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using Conversey.BL.Domain.DTOs.MagicMode;

namespace Tests.IntegrationTests;

public class MockAiManager : IAiManager
{
    public void Dispose()
    {
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "mock")));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public object GetService(Type serviceType, object serviceKey = null)
    {
        return null;
    }

    public Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null)
    {
        return Task.FromResult($"[Mock] Alternative text for: {prompt}");
    }

    public Task<ModerationDecision> ModerateContent(string content)
    {
        return Task.FromResult(new ModerationDecision
        {
            IsAllowed = true,
            Categories = new ModerationInfo()
        });
    }

    public Task<IdeaNudgeDecision> AssessIdeaNudge(IdeaNudgeAssessmentRequest request)
    {
        return Task.FromResult(new IdeaNudgeDecision
        {
            IsApproved = true
        });
    }

    public Task<IEnumerable<int>> RankIdeasByRelation(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
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

        return Task.FromResult(ordered.Take(limit));
    }

    public Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeas(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
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
        string language,
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
        string language)
    {
        if (string.IsNullOrWhiteSpace(transcript) || bubbles == null || bubbles.Count == 0)
            return Task.FromResult(string.Empty);
        
        // Simple mock: combine transcript and bubbles
        return Task.FromResult(transcript + " " + string.Join(", ", bubbles));
    }
}
