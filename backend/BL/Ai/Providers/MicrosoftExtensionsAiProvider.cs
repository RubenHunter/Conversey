using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

//wordt nu niet gebruikt maar is de buildingblock voor implementing de MS stack voor bvb locale modellen en ingebouwde telemetry etc te hebben.
public sealed class MicrosoftExtensionsAiProvider : IAiProvider
{
    private readonly IChatClient _chatClient;
    public string ProviderName { get; }

    public bool SupportsNativeModeration => false;

    public MicrosoftExtensionsAiProvider(IChatClient chatClient, string providerName)
    {
        _chatClient = chatClient;
        ProviderName = providerName;
    }

    public async Task<AiCompletionResult> CompleteAsync(string systemPrompt, string userPrompt, string model, decimal temperature, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            Temperature = (float)temperature,
            ModelId = string.IsNullOrWhiteSpace(model) ? null : model
        };

        var response = await _chatClient.GetResponseAsync(messages, options, cancellationToken);

        return new AiCompletionResult
        {
            Content = response.Messages.FirstOrDefault()?.Text ?? string.Empty,
            PromptTokens = (int)(response.Usage?.InputTokenCount ?? 0),
            CompletionTokens = (int)(response.Usage?.OutputTokenCount ?? 0)
        };
    }

    public Task<AiModerationResult> ModerateAsync(string content, string model, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AiModerationResult { Flagged = false });
    }

    public Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }
}
