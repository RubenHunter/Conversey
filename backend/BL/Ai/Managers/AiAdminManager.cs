using System.Net.Http.Headers;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
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
    private readonly ICostLimitRepository _costLimitRepository;
    private readonly IAiManager _aiManager;
    private readonly IConfiguration _configuration;
    private readonly RateLimitConfigCache _rateLimitCache;

    private static readonly Dictionary<string, AiPrompt> DefaultPrompts = new()
    {
        ["ModerationGenerateAlternative"] = new()
        {
            Name = "ModerationGenerateAlternative",
            SystemPrompt = "You rewrite unsafe user feedback into respectful, constructive feedback while preserving intent. Return only the rewritten text.",
            UserPromptTemplate = "{{IdeaText}}",
            Description = "System prompt for generating a respectful alternative when content is flagged by moderation."
        },
        ["ModerationPrompt"] = new()
        {
            Name = "ModerationPrompt",
            SystemPrompt = "You are a strict content safety classifier for a youth platform. Your task is to flag ANY harmful, toxic, or unsafe content.\n\nAnalyze the text against these categories:\n- sexual: sexually explicit content, sexual harassment, or sexualized language\n- hate_and_discrimination: slurs, hate speech, racism, homophobia, transphobia, bigotry, or discrimination based on identity\n- violence_and_threats: threats of violence, encouragement of violence, or glorification of harm\n- dangerous_and_criminal_content: illegal activity, self-harm instructions, or dangerous pranks\n- self_harm: content promoting or encouraging self-harm or suicide\n- pii: personal identifiable information like phone numbers, addresses, or full names\n\nAlso mark hate_and_discrimination as true for: personal insults involving slurs, name-calling with protected characteristics, profanity-laced harassment, hostile derogatory language, or general offensive/crude language targeting others.\n\nCRITICAL: Be conservative. If you are unsure whether content violates a category, mark it as violating. False positives are safer than false negatives.\n\nReturn ONLY a JSON object with this exact schema:\n{\"flagged\":true,\"categories\":{\"sexual\":false,\"hate_and_discrimination\":true,\"violence_and_threats\":false,\"dangerous_and_criminal_content\":false,\"self_harm\":false,\"pii\":false}}\n\nNo markdown, no code blocks, no explanation — just the raw JSON.",
            UserPromptTemplate = "",
            Description = "Prompt-based content moderation fallback for providers without a dedicated moderation endpoint (non-Mistral). Sends content as user message, expects structured JSON response."
        },
        ["IdeaNudgingSystem"] = new()
        {
            Name = "IdeaNudgingSystem",
            SystemPrompt = "You help youth improve the quality of their idea before publishing. Ask exactly one concrete follow-up question when the idea is too shallow, vague, or underspecified. If the idea is already acceptable for the configured nudging strength, approve it. Never invent multiple questions. Return strict JSON only with the shape {\"isApproved\":true} or {\"isApproved\":false,\"question\":\"...\"}. Nudging strength: {{NudgingModeDescription}}.",
            UserPromptTemplate = "",
            Description = "System prompt for the idea quality nudging assessment. NudgingModeDescription is injected based on the project's nudging strength setting."
        },
        ["IdeaNudgingUser"] = new()
        {
            Name = "IdeaNudgingUser",
            SystemPrompt = "",
            UserPromptTemplate = "Project title: {{ProjectTitle}}\nProject description: {{ProjectDescription}}\nTopic title: {{TopicTitle}}\nTopic prompt/question: {{TopicPrompt}}\n\nCurrent idea draft:\n{{IdeaText}}\n\nConversation so far:\n{{Conversation}}\n\nDecide whether the draft is ready. If not, ask one follow-up question that is specific to this idea and helps deepen it using the project and topic context.",
            Description = "User prompt template for idea nudging. Contains the idea draft, project/topic context, and previous conversation turns."
        },
        ["IdeaRankingSystem"] = new()
        {
            Name = "IdeaRankingSystem",
            SystemPrompt = "You compare youth ideas by meaning. Return only strict JSON with field rankedIndexes as an array of integer indexes. For similarity tasks, return clearly similar ideas. For difference tasks, return ideas with a noticeably different focus or approach; be inclusive rather than restrictive.",
            UserPromptTemplate = "",
            Description = "System prompt for ranking ideas by semantic similarity or difference."
        },
        ["IdeaRankingUser"] = new()
        {
            Name = "IdeaRankingUser",
            SystemPrompt = "",
            UserPromptTemplate = "Reference idea:\n{{ReferenceIdea}}\n\nCandidate ideas (use only these indexes):\n{{Candidates}}\n\nTask:\n- {{RelationGoal}}\n- Return up to {{Limit}} indexes, ordered from best to least fitting for this relation.\n- Do not invent indexes.\n- Return strict JSON only with this schema:\n{\"rankedIndexes\":[0,1,2]}",
            Description = "User prompt template for ranking ideas. Contains reference idea, candidates with indexes, relation goal, and limit."
        },
        ["IdeaCategorizationSystem"] = new()
        {
            Name = "IdeaCategorizationSystem",
            SystemPrompt = "You assign semantic categories to youth ideas. Return only strict JSON.",
            UserPromptTemplate = "",
            Description = "System prompt for assigning semantic category labels to ideas."
        },
        ["IdeaCategorizationUser"] = new()
        {
            Name = "IdeaCategorizationUser",
            SystemPrompt = "",
            UserPromptTemplate = "Categorize each idea semantically. One idea may belong to multiple categories.\n\nThese are the existing categories already used in this topic. Reuse these exact labels whenever possible and only invent a new label if nothing fits:\n{{ExistingCategories}}\n\nIdeas:\n{{Ideas}}\n\nRules:\n- Use short, human-readable category names.\n- Max {{MaxCategoriesPerIdea}} categories per idea.\n- Prefer reusing an existing category label when it is semantically close enough.\n- Avoid near-duplicate labels when an existing category already covers the same meaning.\n- Do not invent idea indexes.\n- Avoid creating near-duplicate labels if an existing category already fits.\n- Return strict JSON only in this shape:\n{\"items\":[{\"index\":0,\"categories\":[\"Category A\",\"Category B\"]}]}",
            Description = "User prompt template for idea categorization. Contains index-labeled ideas, existing category labels, and max categories per idea."
        }
    };

    public AiAdminManager(
        IAuditRepository auditRepository,
        IPromptRepository promptRepository,
        IProviderConfigRepository providerConfigRepository,
        IRateLimitConfigRepository rateLimitConfigRepository,
        IModerationKeywordRepository moderationKeywordRepository,
        ICostLimitRepository costLimitRepository,
        IAiManager aiManager,
        IConfiguration configuration,
        RateLimitConfigCache rateLimitCache)
    {
        _auditRepository = auditRepository;
        _promptRepository = promptRepository;
        _providerConfigRepository = providerConfigRepository;
        _rateLimitConfigRepository = rateLimitConfigRepository;
        _moderationKeywordRepository = moderationKeywordRepository;
        _costLimitRepository = costLimitRepository;
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

    public async Task<AiHealthCheckResult> CheckHealthAsync()
    {
        var result = new AiHealthCheckResult { IsHealthy = true };

        try
        {
            var dbConfigs = await _providerConfigRepository.GetAllConfigsAsync();
            var activeConfig = dbConfigs.FirstOrDefault(c => c.IsEnabled);

            if (activeConfig != null)
            {
                result.ProviderName = activeConfig.ProviderName;
                result.CompletionsModel = activeConfig.CompletionsModel;
                result.ModerationModel = activeConfig.ModerationModel;

                var moderationProbe = await ProbeModerationAsync();
                var completionsProbe = await ProbeCompletionsAsync();

                if (!moderationProbe.Ok || !completionsProbe.Ok)
                {
                    result.IsHealthy = false;
                    var errors = new List<string>();
                    if (!moderationProbe.Ok) errors.Add($"Moderation: {moderationProbe.Error}");
                    if (!completionsProbe.Ok) errors.Add($"Completions: {completionsProbe.Error}");
                    result.Detail = string.Join("; ", errors);
                }
                else
                {
                    result.Detail = "All probes successful";
                }
            }
            else
            {
                var appsettingsProvider = (_configuration["AI:Provider"] ?? "Noop").Trim();
                result.ProviderName = appsettingsProvider;
                result.Detail = appsettingsProvider.Equals("Noop", StringComparison.OrdinalIgnoreCase)
                    ? "Using NoopAiManager (no API calls)"
                    : "No active DB config, using appsettings";
            }
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Detail = ex.Message;
        }

        return result;
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

    public Task<IReadOnlyCollection<AiAuditLog>> GetCostsFilteredAsync(
        string? workspaceId = null,
        string? projectId = null,
        string? modelName = null,
        string? modelType = null,
        string? providerName = null,
        string? promptName = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        return _auditRepository.GetAiCostsFilteredAsync(
            workspaceId, projectId, modelName, modelType, providerName, promptName, dateFrom, dateTo);
    }

    public async Task<AiCostsTimelineSummary> GetCostsTimelineAsync(int days = 30, string? workspaceId = null, string? projectId = null)
    {
        var logs = await _auditRepository.GetAiCostsFilteredAsync(
            workspaceId: workspaceId,
            projectId: projectId,
            dateFrom: DateTime.UtcNow.AddDays(-days));

        var dailyGroups = logs
            .GroupBy(l => l.StartTime.Date)
            .OrderBy(g => g.Key)
            .Select(g => new AiCostsDayEntry
            {
                Date = g.Key,
                Cost = g.Sum(l => l.Cost),
                CallCount = g.Count(),
                InputTokens = g.Sum(l => l.InputTokens),
                OutputTokens = g.Sum(l => l.OutputTokens)
            })
            .ToList();

        return new AiCostsTimelineSummary
        {
            DailyCosts = dailyGroups,
            TotalCost = dailyGroups.Sum(d => d.Cost),
            TotalCalls = dailyGroups.Sum(d => d.CallCount)
        };
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

    public Task<AiPrompt> GetDefaultPromptAsync(string promptName)
    {
        if (DefaultPrompts.TryGetValue(promptName, out var defaultPrompt))
        {
            return Task.FromResult(new AiPrompt
            {
                Name = defaultPrompt.Name,
                SystemPrompt = defaultPrompt.SystemPrompt,
                UserPromptTemplate = defaultPrompt.UserPromptTemplate,
                Description = defaultPrompt.Description
            });
        }

        return Task.FromResult<AiPrompt>(null);
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

        var baseUrl = config.BaseUrl.TrimEnd('/') + '/';
        using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
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

    public async Task<IReadOnlyList<string>> ListProviderModelsFromConfigAsync(AiProviderConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.BaseUrl))
        {
            return Array.Empty<string>();
        }

        var baseUrl = config.BaseUrl.TrimEnd('/') + '/';
        using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
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
        => Task.FromResult(_moderationKeywordRepository.GetAll());

    public Task<IReadOnlyList<ModerationKeyword>> GetModerationKeywordsForWorkspaceAsync(string workspaceId)
        => Task.FromResult(_moderationKeywordRepository.GetForWorkspace(workspaceId));

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

    public Task<AiCostLimit?> GetWorkspaceCostLimitAsync(string workspaceId)
    {
        return _costLimitRepository.GetWorkspaceLimitAsync(workspaceId);
    }

    public Task<AiCostLimit?> GetProjectCostLimitAsync(string projectId)
    {
        return _costLimitRepository.GetProjectLimitAsync(projectId);
    }

    public Task<IReadOnlyList<AiCostLimit>> GetWorkspaceCostLimitsAsync(string workspaceId)
    {
        return _costLimitRepository.GetWorkspaceLimitsAsync(workspaceId);
    }

    public Task<IReadOnlyList<AiCostLimit>> GetProjectCostLimitsAsync(string projectId)
    {
        return _costLimitRepository.GetProjectLimitsAsync(projectId);
    }

    public Task SaveCostLimitAsync(AiCostLimit limit)
    {
        return _costLimitRepository.SaveLimitAsync(limit);
    }

    public Task DeleteCostLimitAsync(int id)
    {
        return _costLimitRepository.DeleteLimitAsync(id);
    }

    public async Task<bool> IsWorkspaceOverLimitAsync(string workspaceId)
    {
        var limit = await _costLimitRepository.GetWorkspaceLimitAsync(workspaceId);
        if (limit == null || !limit.IsActive) return false;

        var totalCost = await _auditRepository.GetTotalCostForWorkspaceAsync(workspaceId, limit.PeriodStart, limit.PeriodEnd);
        return totalCost >= limit.LimitAmount;
    }

    public async Task<bool> IsProjectOverLimitAsync(string projectId)
    {
        var limit = await _costLimitRepository.GetProjectLimitAsync(projectId);
        if (limit == null || !limit.IsActive) return false;

        var totalCost = await _auditRepository.GetTotalCostForProjectAsync(projectId, limit.PeriodStart, limit.PeriodEnd);
        return totalCost >= limit.LimitAmount;
    }

    public Task<Dictionary<string, decimal>> GetCostsPerProjectAsync(string workspaceId, DateTime periodStart, DateTime periodEnd)
    {
        return _auditRepository.GetCostsPerProjectForWorkspaceAsync(workspaceId, periodStart, periodEnd);
    }

    public Task<decimal> GetWorkspaceTotalCostAsync(string workspaceId, DateTime periodStart, DateTime periodEnd)
    {
        return _auditRepository.GetTotalCostForWorkspaceAsync(workspaceId, periodStart, periodEnd);
    }

    public Task<decimal> GetProjectTotalCostAsync(string projectId, DateTime periodStart, DateTime periodEnd)
    {
        return _auditRepository.GetTotalCostForProjectAsync(projectId, periodStart, periodEnd);
    }

    public async Task<IReadOnlyList<string>> GetAllWorkspacesAsync()
    {
        var allLogs = await _auditRepository.GetAiCostsAsync();
        return allLogs
            .Where(l => l.WorkspaceId.HasValue)
            .Select(l => l.WorkspaceId.Value.Text)
            .Distinct()
            .OrderBy(w => w)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetProjectsForWorkspaceAsync(string workspaceId)
    {
        var wsSlug = Slug.FromName(workspaceId);
        var allLogs = await _auditRepository.GetAiCostsAsync();
        return allLogs
            .Where(l => l.WorkspaceId == wsSlug && l.ProjectId.HasValue)
            .Select(l => l.ProjectId.Value.Text)
            .Distinct()
            .OrderBy(p => p)
            .ToList();
    }
}
