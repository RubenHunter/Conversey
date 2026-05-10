using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IAuditRepository
{
    Task LogAiCallAsync(string modelName, string modelType, int inputTokens, int outputTokens, decimal cost, DateTime startTime, TimeSpan duration, string providerName = "", string promptName = "");
    Task<IReadOnlyCollection<AiAuditLog>> GetAiCostsAsync();
}