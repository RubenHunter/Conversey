using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IRateLimitConfigRepository
{
    Task<IReadOnlyList<RateLimitConfig>> GetAllConfigsAsync();
    Task<RateLimitConfig> GetConfigAsync(string policyName);
    Task SaveConfigAsync(RateLimitConfig config);
}
