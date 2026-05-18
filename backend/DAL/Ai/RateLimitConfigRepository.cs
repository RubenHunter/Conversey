using Conversey.BL.Domain.Ai;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Ai;

public class RateLimitConfigRepository : IRateLimitConfigRepository
{
    private readonly ConverseyDbContext _dbContext;

    public RateLimitConfigRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RateLimitConfig>> GetAllConfigsAsync()
    {
        return await _dbContext.RateLimitConfigs.OrderBy(c => c.PolicyName).ToListAsync();
    }

    public Task<RateLimitConfig> GetConfigAsync(string policyName)
    {
        return _dbContext.RateLimitConfigs.FirstOrDefaultAsync(c => c.PolicyName == policyName);
    }

    public async Task SaveConfigAsync(RateLimitConfig config)
    {
        var existing = await _dbContext.RateLimitConfigs.FirstOrDefaultAsync(c => c.Id == config.Id);
        if (existing != null)
        {
            existing.PermitLimit = config.PermitLimit;
            existing.WindowSeconds = config.WindowSeconds;
            existing.QueueLimit = config.QueueLimit;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;
            _dbContext.RateLimitConfigs.Add(config);
        }

        await _dbContext.SaveChangesAsync();
    }
}
