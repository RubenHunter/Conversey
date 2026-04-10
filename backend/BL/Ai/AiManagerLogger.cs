using Conversey.BL.Ai.Services;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.DAL.Subplatform.Ai;
using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public class AiManagerLogger(IAiManager aiManager, IAuditRepository auditRepository) : IAiManager
{

    public async Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null)
    {
        var start = DateTime.UtcNow;
        var alternativeText = await aiManager.GenerateAiAlternative(prompt);
        var duration = DateTime.UtcNow - start;

        try
        {
            var model = GetCurrentModelFromService();
            var outputTokens = alternativeText.Length / 4; // Estimate output tokens
            var cost = CalculateCost(prompt, alternativeText);

            await auditRepository.LogAiCallAsync(
                model.Name,
                model.Type.ToString(),
                prompt.Length,
                outputTokens,
                cost,
                start,
                duration
            );
        }
        catch (Exception ex)
        {
            // Log error but don't break the main functionality
            Console.WriteLine($"Error logging AI call: {ex.Message}");
        }

        return alternativeText;
    }

    public async Task<ModerationDecision> ModerateContent(string prompt)
    {
        var start = DateTime.UtcNow;
        var decision = await aiManager.ModerateContent(prompt);
        var duration = DateTime.UtcNow - start;
        
        try
        {
            var model = GetCurrentModelFromService();
            var outputTokens = 0; // Moderation typically has minimal output
            var cost = CalculateCost(prompt, "moderation-result");

            await auditRepository.LogAiCallAsync(
                model.Name,
                model.Type.ToString(),
                prompt.Length,
                outputTokens,
                cost,
                start,
                duration
            );
        }
        catch (Exception ex)
        {
            // Log error but don't break the main functionality
            Console.WriteLine($"Error logging AI moderation call: {ex.Message}");
        }
        
        return decision;
    }
    
    private AiModel GetCurrentModelFromService()
    {
        if (aiManager is MistralAiManager mistralService)
        {
            return mistralService.CurrentModel;
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