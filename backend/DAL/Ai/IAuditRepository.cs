#nullable enable
using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IAuditRepository
{
    Task LogAiCallAsync(string modelName, string modelType, int inputTokens, int outputTokens, decimal cost, DateTime startTime, TimeSpan duration, string providerName = "", string promptName = "", string? workspaceId = null, string? projectId = null);
    Task<IReadOnlyCollection<AiAuditLog>> GetAiCostsAsync();
    Task<IReadOnlyCollection<AiAuditLog>> GetAiCostsFilteredAsync(
        string? workspaceId = null,
        string? projectId = null,
        string? modelName = null,
        string? modelType = null,
        string? providerName = null,
        string? promptName = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);
    Task<decimal> GetTotalCostForWorkspaceAsync(string workspaceId, DateTime periodStart, DateTime periodEnd);
    Task<decimal> GetTotalCostForProjectAsync(string projectId, DateTime periodStart, DateTime periodEnd);
    Task<Dictionary<string, decimal>> GetCostsPerProjectForWorkspaceAsync(string workspaceId, DateTime periodStart, DateTime periodEnd);
}