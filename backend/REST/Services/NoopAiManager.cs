using Conversey.BL.Ai;
using Microsoft.Extensions.AI;

namespace Conversey.REST.Services;

public sealed class NoopAiManager : IAiManager
{
    private static readonly string[] UnsafeTerms =
    {
        "retarded",
        "moron",
        "dumbass",
        "dumb ass",
        "fucking"
    };

    public void Dispose()
    {
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, "AI integration is currently disabled."));
        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
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
        return Task.FromResult("Please rephrase your message in a respectful way.");
    }

    public Task<ModerationDecision> ModerateContent(string content)
    {
        var normalized = (content ?? string.Empty).Trim().ToLowerInvariant();
        var containsUnsafeLanguage = UnsafeTerms.Any(term => normalized.Contains(term));

        return Task.FromResult(new ModerationDecision
        {
            IsAllowed = !containsUnsafeLanguage,
            Suggestion = containsUnsafeLanguage ? "Please rephrase your idea in a respectful and constructive way." : null
        });
    }
}


