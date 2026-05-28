#nullable enable
using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface ICostLimitRepository
{
    Task<AiCostLimit?> GetWorkspaceLimitAsync(string workspaceId);
    Task<AiCostLimit?> GetProjectLimitAsync(string projectId);
    Task<IReadOnlyList<AiCostLimit>> GetWorkspaceLimitsAsync(string workspaceId);
    Task<IReadOnlyList<AiCostLimit>> GetProjectLimitsAsync(string projectId);
    Task SaveLimitAsync(AiCostLimit limit);
    Task DeleteLimitAsync(int id);
}
