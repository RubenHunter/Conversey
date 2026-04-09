using Conversey.BL.Ai;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Microsoft.Extensions.AI;

namespace Tests.IntegrationTests;

public class MockAIManager : IAiManager
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public object GetService(Type serviceType, object serviceKey = null)
    {
        throw new NotImplementedException();
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
            Categories = new ModerationInfo
            {
                HateAndDiscrimination = false,
                ViolenceAndThreats = false,
                Sexual = false,
                DangerousAndCriminalContent = false,
                SelfHarm = false,
                Pii = false
            }
        });
    }
}