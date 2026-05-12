using System.Net.Http.Headers;
using Conversey.BL.Domain.Ai;
using Conversey.DAL.Subplatform.Ai;
using Microsoft.Extensions.Configuration;

namespace Conversey.BL.Ai;

public sealed class AiAdminManager : IAiAdminManager
{
    private readonly IAuditRepository _auditRepository;
    private readonly IPromptRepository _promptRepository;
    private readonly IProviderConfigRepository _providerConfigRepository;
    private readonly IRateLimitConfigRepository _rateLimitConfigRepository;
    private readonly IModerationKeywordRepository _moderationKeywordRepository;
    private readonly IAiManager _aiManager;
    private readonly IConfiguration _configuration;
    private readonly RateLimitConfigCache _rateLimitCache;

    public AiAdminManager(
        IAuditRepository auditRepository,
        IPromptRepository promptRepository,
        IProviderConfigRepository providerConfigRepository,
        IRateLimitConfigRepository rateLimitConfigRepository,
        IModerationKeywordRepository moderationKeywordRepository,
        IAiManager aiManager,
        IConfiguration configuration,
        RateLimitConfigCache rateLimitCache)
    {
        _auditRepository = auditRepository;
        _promptRepository = promptRepository;
        _providerConfigRepository = providerConfigRepository;
        _rateLimitConfigRepository = rateLimitConfigRepository;
        _moderationKeywordRepository = moderationKeywordRepository;
        _aiManager = aiManager;
        _configuration = configuration;
        _rateLimitCache = rateLimitCache;
    }

    public async Task<AiHealthInfo> GetHealthAsync()
    {
        var appsettingsProvider = (_configuration["AI:Provider"] ?? "Unknown").Trim();
        var managerType = _aiManager.GetType().Name;

        string activeProvider;
        string configSource;
        AiProviderConfig dbConfig = null;

        try
        {
            var dbConfigs = await _providerConfigRepository.GetAllConfigsAsync();
            dbConfig = dbConfigs.FirstOrDefault(c => c.IsEnabled);
        }
        catch
        {
            dbConfig = null;
        }

        if (dbConfig != null)
        {
            activeProvider = dbConfig.ProviderName;
            configSource = "database";
        }
        else
        {
            activeProvider = appsettingsProvider;
            configSource = "appsettings";
        }

        var health = new AiHealthInfo
        {
            ActiveProvider = activeProvider,
            ConfigSource = configSource,
            ManagerType = managerType,
            Moderation = await ProbeModerationAsync(),
            Completions = await ProbeCompletionsAsync(),
            CheckedAtUtc = DateTime.UtcNow
        };

        health.Status = health.Moderation.Ok && health.Completions.Ok ? "ok" : "degraded";
        return health;
    }

    public Task<IReadOnlyCollection<AiAuditLog>> GetAllCostsAsync()
    {
        return _auditRepository.GetAiCostsAsync();
    }

    public async Task<AiCostsSummary> GetCostsSummaryAsync()
    {
        var allCosts = await _auditRepository.GetAiCostsAsync();

        var models = allCosts
            .GroupBy(log => log.ModelName)
            .Select(group => new AiCostsModelSummary
            {
                ModelName = group.Key,
                TotalCost = group.Sum(log => log.Cost),
                CallCount = group.Count(),
                AvgCostPerCall = group.Average(log => log.Cost),
                TotalInputTokens = group.Sum(log => log.InputTokens),
                TotalOutputTokens = group.Sum(log => log.OutputTokens)
            })
            .OrderByDescending(m => m.TotalCost)
            .ToList();

        return new AiCostsSummary
        {
            TotalCost = models.Sum(m => m.TotalCost),
            Models = models.AsReadOnly()
        };
    }

    public async Task<IReadOnlyCollection<AiAuditLog>> GetRecentCostsAsync(int days)
    {
        if (days <= 0 || days > 365)
        {
            throw new ArgumentOutOfRangeException(nameof(days), "Days must be between 1 and 365.");
        }

        var allCosts = await _auditRepository.GetAiCostsAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        return allCosts
            .Where(log => log.StartTime >= cutoffDate)
            .OrderByDescending(log => log.StartTime)
            .ToList()
            .AsReadOnly();
    }

    public Task<IReadOnlyList<AiPrompt>> GetAllPromptsAsync()
    {
        return _promptRepository.GetAllPromptsAsync();
    }

    public async Task<AiPrompt> GetPromptByIdAsync(int id)
    {
        var prompts = await _promptRepository.GetAllPromptsAsync();
        return prompts.FirstOrDefault(p => p.Id == id);
    }

    public async Task SavePromptAsync(AiPrompt prompt)
    {
        prompt.UpdatedAt = DateTime.UtcNow;
        await _promptRepository.SavePromptAsync(prompt);
    }

    public Task<IReadOnlyList<AiProviderConfig>> GetAllProviderConfigsAsync()
    {
        return _providerConfigRepository.GetAllConfigsAsync();
    }

    public async Task<AiProviderConfig> GetProviderConfigByIdAsync(int id)
    {
        var configs = await _providerConfigRepository.GetAllConfigsAsync();
        return configs.FirstOrDefault(c => c.Id == id);
    }

    public async Task SaveProviderConfigAsync(AiProviderConfig config)
    {
        if (config.Temperature == 0m)
        {
            config.Temperature = 1.0m;
        }

        if (config.IsEnabled && config.ApiKeyExpiresAt.HasValue && config.ApiKeyExpiresAt.Value <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot enable a provider whose API key has expired.");
        }

        if (config.IsEnabled)
        {
            var allConfigs = await _providerConfigRepository.GetAllConfigsAsync();
            foreach (var other in allConfigs)
            {
                if (other.Id != config.Id && other.IsEnabled)
                {
                    other.IsEnabled = false;
                    other.UpdatedAt = DateTime.UtcNow;
                    await _providerConfigRepository.SaveConfigAsync(other);
                }
            }
        }

        config.UpdatedAt = DateTime.UtcNow;
        await _providerConfigRepository.SaveConfigAsync(config);
    }

    public Task DeleteProviderConfigAsync(int id)
    {
        return _providerConfigRepository.DeleteConfigAsync(id);
    }

    public async Task<IReadOnlyList<string>> ListProviderModelsAsync(int providerConfigId)
    {
        var configs = await _providerConfigRepository.GetAllConfigsAsync();
        var config = configs.FirstOrDefault(c => c.Id == providerConfigId);
        if (config == null || string.IsNullOrWhiteSpace(config.BaseUrl))
        {
            return Array.Empty<string>();
        }

        using var httpClient = new HttpClient { BaseAddress = new Uri(config.BaseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            if (config.ProviderName.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            {
                httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
            }
        }

        IAiProvider provider = config.ProviderName.Equals("Azure", StringComparison.OrdinalIgnoreCase)
            ? new AzureOpenAiProvider(httpClient, config.CompletionsModel, config.ApiVersion)
            : config.ProviderName.Equals("Mistral", StringComparison.OrdinalIgnoreCase)
                ? new MistralAiProvider(httpClient)
                : new OpenAiCompatibleProvider(httpClient, config.ProviderName);

        return await provider.ListModelsAsync();
    }

    private async Task<AiHealthProbeResult> ProbeModerationAsync()
    {
        var probe = new AiHealthProbeResult();
        var start = DateTime.UtcNow;

        try
        {
            var decision = _aiManager.ModerateContent("health-check: keep this sentence respectful");
            probe.Ok = true;
            probe.IsAllowed = decision.IsAllowed;
        }
        catch (Exception ex)
        {
            probe.Ok = false;
            probe.Error = ex.Message;
            if (ex.InnerException != null)
            {
                probe.InnerError = ex.InnerException.Message;
            }
        }

        probe.DurationMs = (int)(DateTime.UtcNow - start).TotalMilliseconds;
        return probe;
    }

    private async Task<AiHealthProbeResult> ProbeCompletionsAsync()
    {
        var probe = new AiHealthProbeResult();
        var start = DateTime.UtcNow;

        try
        {
            var alternative = _aiManager.GenerateAlternative("test", null);
            probe.Ok = true;
            probe.ResponsePreview = (alternative ?? "").Length > 80 ? alternative[..80] + "..." : alternative;
        }
        catch (Exception ex)
        {
            probe.Ok = false;
            probe.Error = ex.Message;
            if (ex.InnerException != null)
            {
                probe.InnerError = ex.InnerException.Message;
            }
        }

        probe.DurationMs = (int)(DateTime.UtcNow - start).TotalMilliseconds;
        return probe;
    }

    public Task<IReadOnlyList<RateLimitConfig>> GetAllRateLimitConfigsAsync()
    {
        return _rateLimitConfigRepository.GetAllConfigsAsync();
    }

    public async Task<RateLimitConfig> GetRateLimitConfigByIdAsync(int id)
    {
        var configs = await _rateLimitConfigRepository.GetAllConfigsAsync();
        return configs.FirstOrDefault(c => c.Id == id);
    }

    public async Task SaveRateLimitConfigAsync(RateLimitConfig config)
    {
        config.UpdatedAt = DateTime.UtcNow;
        await _rateLimitConfigRepository.SaveConfigAsync(config);
        await _rateLimitCache.ReloadAsync();
    }

    public Task<IReadOnlyList<ModerationKeyword>> GetAllModerationKeywordsAsync()
    {
        return Task.FromResult(_moderationKeywordRepository.GetAll());
    }

    public Task SaveModerationKeywordAsync(ModerationKeyword keyword)
    {
        keyword.Keyword = (keyword.Keyword ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(keyword.Keyword))
        {
            throw new ArgumentException("Keyword cannot be empty.");
        }
        _moderationKeywordRepository.Save(keyword);
        return Task.CompletedTask;
    }

    public Task DeleteModerationKeywordAsync(int id)
    {
        _moderationKeywordRepository.Delete(id);
        return Task.CompletedTask;
    }
}
