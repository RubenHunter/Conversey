using Conversey.DAL.Subplatform.Ai;
using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public class AiSManagerLogger : IAiManager
{
    private readonly IAiManager _aiManager;
    private readonly IAuditRepository _auditRepository; // Zorg dat je deze interface definieert

    public AiSManagerLogger(IAiManager aiManager, IAuditRepository auditRepository)
    {
        _aiManager = aiManager;
        _auditRepository = auditRepository;
    }

    public Task<string> GenerateAiAlternative(string prompt, AiModel model, ModerationDecision decision = null)
    {
        var start = DateTime.UtcNow;
        var alternativeText = _aiManager.GenerateAiAlternative(prompt, model);
        var duration = DateTime.UtcNow -  start;
        var cost = CalculateCost(prompt, "alternative-result", model);

        _auditRepository.LogAiCallAsync(
            model.Name,
            model.Type,
            prompt.Length,
            0,
            cost,
            start,
            duration
        );

        return alternativeText;
    }

    public Task<ModerationDecision> ModerateContent(string prompt, AiModel model)
    {
        var start = DateTime.UtcNow;
        var decision = _aiManager.ModerateContent(prompt, model);
        var duration = DateTime.UtcNow -  start;
        var cost = CalculateCost(prompt, "moderation-result", model);

        _auditRepository.LogAiCallAsync(
            model.Name,
            model.Type,
            prompt.Length,
            0,
            cost,
            start,
            duration
        );

        return decision;
    }
    
    private decimal CalculateCost(string input, string output, AiModel model)
    {
        // Simpele kostencalculatie (pas aan op basis van je prijzen bij Mistral)
        var inputTokens = input.Length / 4; // Ruwe schatting
        var outputTokens = output.Length / 4; // Ruwe schatting
        return (inputTokens + outputTokens) * 0.0001m; // Pas de prijs aan
    }

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
}