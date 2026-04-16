using Conversey.BL.Ai;
using Conversey.BL.Domain.Ideation;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

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
}