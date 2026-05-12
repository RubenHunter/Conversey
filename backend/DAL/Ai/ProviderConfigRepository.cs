using Conversey.BL.Domain.Ai;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Ai;

public class ProviderConfigRepository : IProviderConfigRepository
{
    private const string ProtectedPrefix = "ENC:";
    private readonly ConverseyDbContext _dbContext;
    private readonly IDataProtector _protector;

    public ProviderConfigRepository(ConverseyDbContext dbContext, IDataProtectionProvider dataProtectionProvider)
    {
        _dbContext = dbContext;
        _protector = dataProtectionProvider.CreateProtector("Conversey.AiProviderConfig.ApiKey");
    }

    public async Task<IReadOnlyList<AiProviderConfig>> GetAllConfigsAsync()
    {
        var configs = await _dbContext.AiProviderConfigs.OrderBy(c => c.ProviderName).ToListAsync();
        foreach (var config in configs)
        {
            config.ApiKey = Decrypt(config.ApiKey);
        }
        return configs;
    }

    public async Task<AiProviderConfig> GetActiveConfigAsync(string providerName)
    {
        var config = await _dbContext.AiProviderConfigs
            .FirstOrDefaultAsync(c => c.ProviderName == providerName && c.IsEnabled);
        if (config != null)
        {
            config.ApiKey = Decrypt(config.ApiKey);
        }
        return config;
    }

    public async Task SaveConfigAsync(AiProviderConfig config)
    {
        config.ApiKey = Encrypt(config.ApiKey);

        var existing = await _dbContext.AiProviderConfigs.FirstOrDefaultAsync(c => c.Id == config.Id);
        if (existing != null)
        {
            existing.ProviderName = config.ProviderName;
            existing.BaseUrl = config.BaseUrl;
            existing.ApiKey = config.ApiKey;
            existing.ApiVersion = config.ApiVersion;
            existing.CompletionsModel = config.CompletionsModel;
            existing.ModerationModel = config.ModerationModel;
            existing.Temperature = config.Temperature;
            existing.IsEnabled = config.IsEnabled;
            existing.ApiKeyExpiresAt = config.ApiKeyExpiresAt.HasValue && config.ApiKeyExpiresAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(config.ApiKeyExpiresAt.Value, DateTimeKind.Utc)
                : config.ApiKeyExpiresAt;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            if (config.ApiKeyExpiresAt.HasValue && config.ApiKeyExpiresAt.Value.Kind == DateTimeKind.Unspecified)
            {
                config.ApiKeyExpiresAt = DateTime.SpecifyKind(config.ApiKeyExpiresAt.Value, DateTimeKind.Utc);
            }
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

    private string Encrypt(string value)
    {
        if (string.IsNullOrEmpty(value) || value.StartsWith(ProtectedPrefix))
        {
            return value;
        }
        return ProtectedPrefix + _protector.Protect(value);
    }

    private string Decrypt(string value)
    {
        if (string.IsNullOrEmpty(value) || !value.StartsWith(ProtectedPrefix))
        {
            return value;
        }
        try
        {
            return _protector.Unprotect(value[ProtectedPrefix.Length..]);
        }
        catch
        {
            return value;
        }
    }
}
