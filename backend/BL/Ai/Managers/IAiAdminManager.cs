using Conversey.BL.Domain.Ai;

namespace Conversey.BL.Ai;

public interface IAiAdminManager
{
    Task<AiHealthInfo> GetHealthAsync();
    Task<AiHealthCheckResult> CheckHealthAsync();
    Task<IReadOnlyCollection<AiAuditLog>> GetAllCostsAsync();
    Task<AiCostsSummary> GetCostsSummaryAsync();
    Task<IReadOnlyCollection<AiAuditLog>> GetRecentCostsAsync(int days);
    Task<IReadOnlyCollection<AiAuditLog>> GetCostsFilteredAsync(
        string? workspaceId = null,
        string? projectId = null,
        string? modelName = null,
        string? modelType = null,
        string? providerName = null,
        string? promptName = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);
    Task<AiCostsTimelineSummary> GetCostsTimelineAsync(int days = 30, string? workspaceId = null, string? projectId = null);
    Task<IReadOnlyList<AiPrompt>> GetAllPromptsAsync();
    Task<AiPrompt> GetPromptByIdAsync(int id);
    Task SavePromptAsync(AiPrompt prompt);
    Task<AiPrompt> GetDefaultPromptAsync(string promptName);
    Task<IReadOnlyList<AiProviderConfig>> GetAllProviderConfigsAsync();
    Task<AiProviderConfig> GetProviderConfigByIdAsync(int id);
    Task SaveProviderConfigAsync(AiProviderConfig config);
    Task DeleteProviderConfigAsync(int id);
    Task<IReadOnlyList<string>> ListProviderModelsAsync(int providerConfigId);
    Task<IReadOnlyList<string>> ListProviderModelsFromConfigAsync(AiProviderConfig config);
    Task<IReadOnlyList<RateLimitConfig>> GetAllRateLimitConfigsAsync();
    Task<RateLimitConfig> GetRateLimitConfigByIdAsync(int id);
    Task SaveRateLimitConfigAsync(RateLimitConfig config);
    Task<IReadOnlyList<ModerationKeyword>> GetAllModerationKeywordsAsync();
    Task SaveModerationKeywordAsync(ModerationKeyword keyword);
    Task DeleteModerationKeywordAsync(int id);
    Task<AiCostLimit?> GetWorkspaceCostLimitAsync(string workspaceId);
    Task<AiCostLimit?> GetProjectCostLimitAsync(string projectId);
    Task<IReadOnlyList<AiCostLimit>> GetWorkspaceCostLimitsAsync(string workspaceId);
    Task<IReadOnlyList<AiCostLimit>> GetProjectCostLimitsAsync(string projectId);
    Task SaveCostLimitAsync(AiCostLimit limit);
    Task DeleteCostLimitAsync(int id);
    Task<bool> IsWorkspaceOverLimitAsync(string workspaceId);
    Task<bool> IsProjectOverLimitAsync(string projectId);
    Task<Dictionary<string, decimal>> GetCostsPerProjectAsync(string workspaceId, DateTime periodStart, DateTime periodEnd);
    Task<decimal> GetWorkspaceTotalCostAsync(string workspaceId, DateTime periodStart, DateTime periodEnd);
    Task<decimal> GetProjectTotalCostAsync(string projectId, DateTime periodStart, DateTime periodEnd);
    Task<IReadOnlyList<string>> GetAllWorkspacesAsync();
    Task<IReadOnlyList<string>> GetProjectsForWorkspaceAsync(string workspaceId);
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

public class AiHealthCheckResult
{
    public bool IsHealthy { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string CompletionsModel { get; set; } = string.Empty;
    public string ModerationModel { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}

public class AiHealthProbeResult
{
    public bool Ok { get; set; }
    public string Error { get; set; } = string.Empty;
    public string InnerError { get; set; } = string.Empty;
    public int DurationMs { get; set; }
    public string ResponsePreview { get; set; } = string.Empty;
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

public class AiCostsTimelineSummary
{
    public List<AiCostsDayEntry> DailyCosts { get; set; } = new();
    public decimal TotalCost { get; set; }
    public int TotalCalls { get; set; }
}

public class AiCostsDayEntry
{
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
    public int CallCount { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}
