using Conversey.DAL.Subplatform.Ai;
using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public class AiManagerLogger(IAiManager aiManager, IAuditRepository auditRepository) : IAiManager
{

    public Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null)
    {
        var start = DateTime.UtcNow;
        var alternativeText = aiManager.GenerateAiAlternative(prompt);
        var duration = DateTime.UtcNow -  start;

        var model = GetCurrentModelFromService();
        var cost = CalculateCost(prompt, "alternative-result");

        auditRepository.LogAiCallAsync(
            model.Name,
            model.Type.ToString(),
            prompt.Length,
            0,
            cost,
            start,
            duration
        );

        return alternativeText;
    }

    public Task<ModerationDecision> ModerateContent(string prompt)
    {
        var start = DateTime.UtcNow;
        var decision = aiManager.ModerateContent(prompt);
        var duration = DateTime.UtcNow -  start;
        
        var model = GetCurrentModelFromService();
        var cost = CalculateCost(prompt, "moderation-result");

        auditRepository.LogAiCallAsync(
            model.Name,
            model.Type.ToString(),
            prompt.Length,
            0,
            cost,
            start,
            duration
        );

        return decision;
    }
    
    private AiModel GetCurrentModelFromService()
    {
        if (aiManager is MistralAiManager mistralManager)
        {
            return mistralManager.CurrentModel;
        }

        // Als het een andere provider is, retourneer een default model
        return new AiModel { Name = "unknown", Type = ModelType.Unknown };
    }
    
    private decimal CalculateCost(string input, string output)
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