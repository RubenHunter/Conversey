using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;

namespace Conversey.UI_MVC.Models.AiAdmin;

public class AiProvidersViewModel
{
    public IReadOnlyList<AiProviderConfig> Providers { get; set; } = Array.Empty<AiProviderConfig>();
    public AiHealthCheckResult HealthCheck { get; set; } = new();
}

public class AiProviderFormViewModel
{
    public int Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string CompletionsModel { get; set; } = string.Empty;
    public string ModerationModel { get; set; } = string.Empty;
    public string SttModel { get; set; } = string.Empty;
    public string TtsModel { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = string.Empty;
    public decimal Temperature { get; set; } = 1.0m;
    public bool IsEnabled { get; set; }
    public DateTime? ApiKeyExpiresAt { get; set; }
}

public class AiModelsViewModel
{
    public AiProviderConfig Provider { get; set; } = new();
    public IReadOnlyList<string> AvailableModels { get; set; } = Array.Empty<string>();
    public string? FetchError { get; set; }
}
