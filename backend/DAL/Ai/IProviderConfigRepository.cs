using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IProviderConfigRepository
{
    Task<IReadOnlyList<AiProviderConfig>> GetAllConfigsAsync();
    Task<AiProviderConfig> GetActiveConfigAsync(string providerName);
    Task SaveConfigAsync(AiProviderConfig config);
    Task DeleteConfigAsync(int id);
}
