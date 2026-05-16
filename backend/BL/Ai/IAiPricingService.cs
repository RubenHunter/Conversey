using Conversey.BL.Domain.Ai;

namespace Conversey.BL.Ai;

public interface IAiPricingService
{
    Task<decimal> CalculateCostAsync(string modelName, int inputTokens, int outputTokens);
    Task RefreshPricingAsync();
    Task<decimal> GetEurExchangeRateAsync();
    Task<IReadOnlyList<AiModelPricing>> GetAllPricingAsync();
}
