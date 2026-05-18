using Conversey.BL.Domain.Ai;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Ai;

public class ModelPricingRepository : IModelPricingRepository
{
    private readonly ConverseyDbContext _dbContext;

    public ModelPricingRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AiModelPricing?> GetPricingAsync(string modelName)
    {
        return await _dbContext.AiModelPricings
            .FirstOrDefaultAsync(p => p.ModelName == modelName);
    }

    public async Task<IReadOnlyList<AiModelPricing>> GetAllPricingAsync()
    {
        return await _dbContext.AiModelPricings
            .OrderBy(p => p.ModelName)
            .ToListAsync();
    }

    public async Task SavePricingAsync(AiModelPricing pricing)
    {
        pricing.UpdatedAt = DateTime.UtcNow;
        var existing = await _dbContext.AiModelPricings
            .FirstOrDefaultAsync(p => p.ModelName == pricing.ModelName);
        if (existing != null)
        {
            existing.InputPricePerMillionTokens = pricing.InputPricePerMillionTokens;
            existing.OutputPricePerMillionTokens = pricing.OutputPricePerMillionTokens;
            existing.ProviderName = pricing.ProviderName;
            existing.UpdatedAt = pricing.UpdatedAt;
        }
        else
        {
            _dbContext.AiModelPricings.Add(pricing);
        }
        await _dbContext.SaveChangesAsync();
    }

    public async Task SavePricingBatchAsync(IReadOnlyList<AiModelPricing> pricings)
    {
        foreach (var pricing in pricings)
        {
            pricing.UpdatedAt = DateTime.UtcNow;
            var existing = await _dbContext.AiModelPricings
                .FirstOrDefaultAsync(p => p.ModelName == pricing.ModelName);
            if (existing != null)
            {
                existing.InputPricePerMillionTokens = pricing.InputPricePerMillionTokens;
                existing.OutputPricePerMillionTokens = pricing.OutputPricePerMillionTokens;
                existing.ProviderName = pricing.ProviderName;
                existing.UpdatedAt = pricing.UpdatedAt;
            }
            else
            {
                _dbContext.AiModelPricings.Add(pricing);
            }
        }
        await _dbContext.SaveChangesAsync();
    }

    public async Task ClearAllAsync()
    {
        var all = await _dbContext.AiModelPricings.ToListAsync();
        _dbContext.AiModelPricings.RemoveRange(all);
        await _dbContext.SaveChangesAsync();
    }
}
