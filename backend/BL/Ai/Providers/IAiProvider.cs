namespace Conversey.BL.Ai;

public interface IAiProvider
{
    string ProviderName { get; }

    bool SupportsNativeModeration { get; }

    Task<AiCompletionResult> CompleteAsync(string systemPrompt, string userPrompt, string model, decimal temperature, CancellationToken cancellationToken = default);

    Task<AiModerationResult> ModerateAsync(string content, string model, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default);
}
