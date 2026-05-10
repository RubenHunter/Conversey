using Conversey.BL.Domain.Ai;

namespace Conversey.BL.Ai;

public interface IAiAdminManager
{
    Task<AiHealthInfo> GetHealthAsync();
    Task<IReadOnlyCollection<AiAuditLog>> GetAllCostsAsync();
    Task<AiCostsSummary> GetCostsSummaryAsync();
    Task<IReadOnlyCollection<AiAuditLog>> GetRecentCostsAsync(int days);
    Task<IReadOnlyList<AiPrompt>> GetAllPromptsAsync();
    Task<AiPrompt> GetPromptByIdAsync(int id);
    Task SavePromptAsync(AiPrompt prompt);
    Task<IReadOnlyList<AiProviderConfig>> GetAllProviderConfigsAsync();
    Task<AiProviderConfig> GetProviderConfigByIdAsync(int id);
    Task SaveProviderConfigAsync(AiProviderConfig config);
    Task DeleteProviderConfigAsync(int id);
    Task<IReadOnlyList<string>> ListProviderModelsAsync(int providerConfigId);
}

public class AiHealthInfo
{
    public string Status { get; set; } = string.Empty;
    public string ActiveProvider { get; set; } = string.Empty;
    public string ConfigSource { get; set; } = string.Empty;
    public string ManagerType { get; set; } = string.Empty;
    public AiHealthProbeResult Moderation { get; set; } = new();
    public AiHealthProbeResult Completions { get; set; } = new();
    public DateTime CheckedAtUtc { get; set; }
}

public class AiHealthProbeResult
{
    public bool Ok { get; set; }
    public string Error { get; set; }
    public string InnerError { get; set; }
    public int DurationMs { get; set; }
    public string ResponsePreview { get; set; }
    public bool? IsAllowed { get; set; }
}

public class AiCostsSummary
{
    public decimal TotalCost { get; set; }
    public string Currency { get; set; } = "EUR";
    public IReadOnlyList<AiCostsModelSummary> Models { get; set; } = Array.Empty<AiCostsModelSummary>();
}

public class AiCostsModelSummary
{
    public string ModelName { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int CallCount { get; set; }
    public decimal AvgCostPerCall { get; set; }
    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
}
