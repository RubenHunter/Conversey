using Conversey.BL.Domain.Ai;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Ai;

public class ProviderConfigRepository : IProviderConfigRepository
{
    private readonly ConverseyDbContext _dbContext;

    public ProviderConfigRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AiProviderConfig>> GetAllConfigsAsync()
    {
        return await _dbContext.AiProviderConfigs.OrderBy(c => c.ProviderName).ToListAsync();
    }

    public Task<AiProviderConfig> GetActiveConfigAsync(string providerName)
    {
        return _dbContext.AiProviderConfigs
            .FirstOrDefaultAsync(c => c.ProviderName == providerName && c.IsEnabled);
    }

    public async Task SaveConfigAsync(AiProviderConfig config)
    {
        var existing = await _dbContext.AiProviderConfigs.FirstOrDefaultAsync(c => c.Id == config.Id);
        if (existing != null)
        {
            existing.ProviderName = config.ProviderName;
            existing.BaseUrl = config.BaseUrl;
            existing.ApiKey = config.ApiKey;
            existing.CompletionsModel = config.CompletionsModel;
            existing.ModerationModel = config.ModerationModel;
            existing.IsEnabled = config.IsEnabled;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;
            _dbContext.AiProviderConfigs.Add(config);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteConfigAsync(int id)
    {
        var config = await _dbContext.AiProviderConfigs.FindAsync(id);
        if (config != null)
        {
            _dbContext.AiProviderConfigs.Remove(config);
            await _dbContext.SaveChangesAsync();
        }
    }
}
