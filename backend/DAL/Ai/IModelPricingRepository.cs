#nullable enable
using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IModelPricingRepository
{
    Task<AiModelPricing?> GetPricingAsync(string modelName);
    Task<IReadOnlyList<AiModelPricing>> GetAllPricingAsync();
    Task SavePricingAsync(AiModelPricing pricing);
    Task SavePricingBatchAsync(IReadOnlyList<AiModelPricing> pricings);
    Task ClearAllAsync();
}
