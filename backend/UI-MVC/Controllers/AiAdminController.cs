using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;
using Conversey.UI_MVC.Models.AiAdmin;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers;

[Authorize(Policy = ConverseyAdminPolicy.Name)]
public class AiAdminController : Controller
{
    private readonly IAiAdminManager _aiAdminManager;
    private readonly IAiPricingService _pricingService;

    public AiAdminController(IAiAdminManager aiAdminManager, IAiPricingService pricingService)
    {
        _aiAdminManager = aiAdminManager;
        _pricingService = pricingService;
    }

    [HttpGet]
    [Route("admin/ai")]
    public async Task<IActionResult> Index()
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var monthLogs = await _aiAdminManager.GetCostsFilteredAsync(dateFrom: monthStart, dateTo: monthEnd);
        var costsSummary = new AiCostsSummary
        {
            TotalCost = monthLogs.Sum(l => l.Cost),
            Models = monthLogs
                .GroupBy(l => l.ModelName)
                .Select(g => new AiCostsModelSummary
                {
                    ModelName = g.Key,
                    TotalCost = g.Sum(l => l.Cost),
                    CallCount = g.Count(),
                    AvgCostPerCall = g.Average(l => l.Cost),
                    TotalInputTokens = g.Sum(l => l.InputTokens),
                    TotalOutputTokens = g.Sum(l => l.OutputTokens)
                })
                .OrderByDescending(m => m.TotalCost)
                .ToList()
        };

        var providers = await _aiAdminManager.GetAllProviderConfigsAsync();
        var prompts = await _aiAdminManager.GetAllPromptsAsync();

        var model = new AiDashboardViewModel
        {
            CostsSummary = costsSummary,
            TotalProviders = providers.Count,
            TotalPrompts = prompts.Count,
            HealthCheck = new AiHealthCheckResult { IsHealthy = true, Detail = "Checking..." }
        };

        return View(model);
    }

    [HttpGet]
    [Route("admin/ai/costs")]
    public async Task<IActionResult> Costs([FromQuery] AiCostsFilterViewModel filter)
    {
        var auditLogs = await _aiAdminManager.GetCostsFilteredAsync(
            workspaceId: filter?.WorkspaceId,
            modelName: filter?.ModelName,
            modelType: filter?.ModelType,
            providerName: filter?.ProviderName,
            promptName: filter?.PromptName,
            dateFrom: filter?.DateFrom.HasValue == true ? DateTime.SpecifyKind(filter.DateFrom.Value, DateTimeKind.Utc) : null,
            dateTo: filter?.DateTo.HasValue == true ? DateTime.SpecifyKind(filter.DateTo.Value, DateTimeKind.Utc) : null);

        var timeline = await _aiAdminManager.GetCostsTimelineAsync(
            days: filter?.TimelineDays ?? 30,
            workspaceId: filter?.WorkspaceId);

        var summary = new AiCostsSummary
        {
            TotalCost = auditLogs.Sum(l => l.Cost),
            Models = auditLogs
                .GroupBy(l => l.ModelName)
                .Select(g => new AiCostsModelSummary
                {
                    ModelName = g.Key,
                    TotalCost = g.Sum(l => l.Cost),
                    CallCount = g.Count(),
                    AvgCostPerCall = g.Average(l => l.Cost),
                    TotalInputTokens = g.Sum(l => l.InputTokens),
                    TotalOutputTokens = g.Sum(l => l.OutputTokens)
                })
                .OrderByDescending(m => m.TotalCost)
                .ToList()
        };

        var allLogs = await _aiAdminManager.GetAllCostsAsync();
        var workspaces = await _aiAdminManager.GetAllWorkspacesAsync();
        var providers = await _aiAdminManager.GetAllProviderConfigsAsync();
        var prompts = await _aiAdminManager.GetAllPromptsAsync();

        var model = new AiCostsViewModel
        {
            Timeline = timeline,
            Summary = summary,
            AuditLogs = auditLogs,
            Workspaces = workspaces,
            Models = allLogs.Select(l => l.ModelName).Distinct().OrderBy(m => m).ToList(),
            ModelTypes = allLogs.Select(l => l.ModelType).Distinct().OrderBy(m => m).ToList(),
            Providers = providers.Select(p => p.ProviderName).Distinct().OrderBy(p => p).ToList(),
            Prompts = prompts.Select(p => p.Name).Distinct().OrderBy(p => p).ToList(),
            Filter = filter ?? new AiCostsFilterViewModel()
        };

        return View("Costs/Costs", model);
    }

    [HttpGet]
    [Route("admin/ai/providers")]
    public async Task<IActionResult> Providers([FromQuery] bool? setup)
    {
        if (setup == true)
        {
            return View("Providers/Setup");
        }

        var providers = await _aiAdminManager.GetAllProviderConfigsAsync();
        var healthCheck = await _aiAdminManager.CheckHealthAsync();

        var model = new AiProvidersViewModel
        {
            Providers = providers,
            HealthCheck = healthCheck
        };

        return View("Providers/Providers", model);
    }

    [HttpGet]
    [Route("admin/ai/providers/new")]
    public IActionResult CreateProvider()
    {
        return View("Providers/EditProvider", new AiProviderFormViewModel());
    }

    [HttpGet]
    [Route("admin/ai/providers/{id:int}/edit")]
    public async Task<IActionResult> EditProvider(int id)
    {
        var config = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (config == null) return NotFound();

        var model = new AiProviderFormViewModel
        {
            Id = config.Id,
            ProviderName = config.ProviderName,
            BaseUrl = config.BaseUrl,
            ApiKey = config.ApiKey,
            CompletionsModel = config.CompletionsModel,
            ModerationModel = config.ModerationModel,
            ApiVersion = config.ApiVersion,
            Temperature = config.Temperature,
            IsEnabled = config.IsEnabled,
            ApiKeyExpiresAt = config.ApiKeyExpiresAt
        };

        return View("Providers/EditProvider", model);
    }

    [HttpPost]
    [Route("admin/ai/providers/{id:int?}")]
    public async Task<IActionResult> SaveProvider(int? id, AiProviderFormViewModel form)
    {
        if (!ModelState.IsValid) return View("Providers/EditProvider", form);

        var config = id.HasValue && id.Value > 0
            ? await _aiAdminManager.GetProviderConfigByIdAsync(id.Value)
            : new AiProviderConfig();

        if (id.HasValue && id.Value > 0 && config == null) return NotFound();

        config.ProviderName = form.ProviderName;
        config.BaseUrl = form.BaseUrl;
        config.CompletionsModel = form.CompletionsModel;
        config.ModerationModel = form.ModerationModel;
        config.ApiVersion = form.ApiVersion;
        config.Temperature = form.Temperature;
        config.IsEnabled = form.IsEnabled;
        config.ApiKeyExpiresAt = form.ApiKeyExpiresAt.HasValue
            ? DateTime.SpecifyKind(form.ApiKeyExpiresAt.Value, DateTimeKind.Utc)
            : null;

        if (!string.IsNullOrWhiteSpace(form.ApiKey) && form.ApiKey != new string('*', form.ApiKey.Length))
        {
            config.ApiKey = form.ApiKey;
        }

        await _aiAdminManager.SaveProviderConfigAsync(config);

        if (!id.HasValue || id.Value <= 0)
        {
            return RedirectToAction("Providers", new { setupCleared = 1 });
        }

        return RedirectToAction("Providers");
    }

    [HttpPost]
    [Route("admin/ai/providers/{id:int}/delete")]
    public async Task<IActionResult> DeleteProvider(int id)
    {
        var config = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (config == null) return NotFound();

        try
        {
            await _aiAdminManager.DeleteProviderConfigAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        return RedirectToAction("Providers");
    }

    [HttpGet]
    [Route("admin/ai/providers/{id:int}/models")]
    public async Task<IActionResult> ConfigureModels(int id)
    {
        var config = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (config == null) return NotFound();

        IReadOnlyList<string> availableModels = Array.Empty<string>();
        string? fetchError = null;

        try
        {
            availableModels = await _aiAdminManager.ListProviderModelsAsync(id);
        }
        catch (Exception ex)
        {
            fetchError = ex.Message;
        }

        var model = new AiModelsViewModel
        {
            Provider = config,
            AvailableModels = availableModels,
            FetchError = fetchError
        };
        return View("Providers/ConfigureModels", model);
    }

    [HttpPost]
    [Route("admin/ai/providers/{id:int}/models")]
    public async Task<IActionResult> SaveModels(int id, string CompletionsModel, string ModerationModel, decimal Temperature)
    {
        var existing = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (existing == null) return NotFound();

        existing.CompletionsModel = CompletionsModel;
        existing.ModerationModel = ModerationModel;
        existing.Temperature = Temperature;

        await _aiAdminManager.SaveProviderConfigAsync(existing);
        return RedirectToAction("Providers");
    }

    [HttpPost]
    [Route("admin/ai/providers/test-models")]
    public async Task<IActionResult> TestModels([FromBody] AiProviderFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.BaseUrl))
        {
            return BadRequest("Base URL is required.");
        }

        var config = new AiProviderConfig
        {
            ProviderName = form.ProviderName ?? "Custom",
            BaseUrl = form.BaseUrl,
            ApiKey = form.ApiKey ?? string.Empty,
            ApiVersion = form.ApiVersion ?? string.Empty,
            CompletionsModel = form.CompletionsModel ?? string.Empty
        };

        IReadOnlyList<string> models = Array.Empty<string>();
        string? error = null;

        try
        {
            models = await _aiAdminManager.ListProviderModelsFromConfigAsync(config);
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        return Json(new { models, error });
    }

    [HttpPost]
    [Route("admin/ai/providers/test-health")]
    public async Task<IActionResult> TestHealth([FromBody] AiProviderFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.BaseUrl))
        {
            return BadRequest("Base URL is required.");
        }

        var config = new AiProviderConfig
        {
            ProviderName = form.ProviderName ?? "Custom",
            BaseUrl = form.BaseUrl,
            ApiKey = form.ApiKey ?? string.Empty,
            ApiVersion = form.ApiVersion ?? string.Empty,
            CompletionsModel = form.CompletionsModel ?? string.Empty,
            ModerationModel = form.ModerationModel ?? string.Empty,
            Temperature = form.Temperature
        };

        var (moderationProbe, completionsProbe) = await _aiAdminManager.ProbeProviderAsync(config);
        int modelCount = 0;
        try
        {
            var models = await _aiAdminManager.ListProviderModelsFromConfigAsync(config);
            modelCount = models.Count;
        }
        catch { }

        return Json(new
        {
            healthy = moderationProbe.Ok && completionsProbe.Ok,
            modelCount,
            moderation = new { ok = moderationProbe.Ok, error = moderationProbe.Error, durationMs = moderationProbe.DurationMs },
            completions = new { ok = completionsProbe.Ok, error = completionsProbe.Error, preview = completionsProbe.ResponsePreview, durationMs = completionsProbe.DurationMs }
        });
    }

    [HttpGet]
    [Route("admin/ai/prompts")]
    public async Task<IActionResult> Prompts([FromQuery] string? search)
    {
        var prompts = await _aiAdminManager.GetAllPromptsAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            prompts = prompts
                .Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var model = new AiPromptsViewModel
        {
            Prompts = prompts,
            SearchQuery = search ?? string.Empty
        };

        return View("Prompts/Prompts", model);
    }

    [HttpGet]
    [Route("admin/ai/prompts/{id:int}/edit")]
    public async Task<IActionResult> EditPrompt(int id)
    {
        var prompt = await _aiAdminManager.GetPromptByIdAsync(id);
        if (prompt == null) return NotFound();

        var defaultPrompt = await _aiAdminManager.GetDefaultPromptAsync(prompt.Name);

        var model = new AiPromptEditViewModel
        {
            Prompt = prompt,
            DefaultPrompt = defaultPrompt ?? new AiPrompt(),
            HasDefault = defaultPrompt != null
        };

        return View("Prompts/EditPrompt", model);
    }

    [HttpPost]
    [Route("admin/ai/prompts/{id:int}")]
    public async Task<IActionResult> SavePrompt(int id, string SystemPrompt, string UserPromptTemplate, string Description)
    {
        if (!ModelState.IsValid) return RedirectToAction("EditPrompt", new { id });

        var existing = await _aiAdminManager.GetPromptByIdAsync(id);
        if (existing == null) return NotFound();

        existing.SystemPrompt = SystemPrompt;
        existing.UserPromptTemplate = UserPromptTemplate;
        existing.Description = Description;

        await _aiAdminManager.SavePromptAsync(existing);
        return RedirectToAction("Prompts");
    }

    [HttpPost]
    [Route("admin/ai/prompts/{id:int}/reset")]
    public async Task<IActionResult> ResetPrompt(int id)
    {
        var prompt = await _aiAdminManager.GetPromptByIdAsync(id);
        if (prompt == null) return NotFound();

        var defaultPrompt = await _aiAdminManager.GetDefaultPromptAsync(prompt.Name);
        if (defaultPrompt == null) return BadRequest("No default available for this prompt.");

        prompt.SystemPrompt = defaultPrompt.SystemPrompt;
        prompt.UserPromptTemplate = defaultPrompt.UserPromptTemplate;
        prompt.Description = defaultPrompt.Description;

        await _aiAdminManager.SavePromptAsync(prompt);
        return RedirectToAction("EditPrompt", new { id });
    }

    [HttpGet]
    [Route("admin/ai/keywords")]
    public async Task<IActionResult> Keywords()
    {
        var keywords = await _aiAdminManager.GetAllModerationKeywordsAsync();
        var model = new AiKeywordsViewModel
        {
            Keywords = keywords.Select(k => new AiKeywordItem
            {
                Id = k.Id,
                Keyword = k.Keyword,
                WorkspaceId = k.WorkspaceId,
                CreatedAt = k.CreatedAt.ToString("yyyy-MM-dd")
            }).ToList()
        };
        return View("Keywords/Keywords", model);
    }

    [HttpPost]
    [Route("admin/ai/keywords/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateKeyword(string Keyword)
    {
        if (string.IsNullOrWhiteSpace(Keyword))
            return RedirectToAction("Keywords");

        await _aiAdminManager.SaveModerationKeywordAsync(new ModerationKeyword
        {
            Keyword = Keyword.Trim(),
            WorkspaceId = null
        });
        return RedirectToAction("Keywords");
    }

    [HttpPost]
    [Route("admin/ai/keywords/{id:int}/delete")]
    public async Task<IActionResult> DeleteKeyword(int id)
    {
        await _aiAdminManager.DeleteModerationKeywordAsync(id);
        return RedirectToAction("Keywords");
    }

    [HttpGet]
    [Route("admin/ai/pricing")]
    public async Task<IActionResult> Pricing()
    {
        var pricing = await _pricingService.GetAllPricingAsync();
        var eurRate = await _pricingService.GetEurExchangeRateAsync();
        ViewBag.EurRate = eurRate;
        return View("Pricing/Pricing", pricing);
    }

    [HttpPost]
    [Route("admin/ai/pricing/refresh")]
    public async Task<IActionResult> RefreshPricing()
    {
        await _pricingService.RefreshPricingAsync();
        return RedirectToAction("Pricing");
    }

    [HttpGet]
    [Route("admin/ai/rate-limits")]
    public async Task<IActionResult> RateLimits()
    {
        var configs = await _aiAdminManager.GetAllRateLimitConfigsAsync();
        return View("RateLimits/RateLimits", configs);
    }

    [HttpGet]
    [Route("admin/ai/rate-limits/{id:int}")]
    public async Task<IActionResult> EditRateLimit(int id)
    {
        var config = await _aiAdminManager.GetRateLimitConfigByIdAsync(id);
        if (config == null) return NotFound();

        return View("RateLimits/EditRateLimit", config);
    }

    [HttpPost]
    [Route("admin/ai/rate-limits/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRateLimit(int id, RateLimitConfig form)
    {
        if (!ModelState.IsValid) return View("RateLimits/EditRateLimit", form);

        var existing = await _aiAdminManager.GetRateLimitConfigByIdAsync(id);
        if (existing == null) return NotFound();

        existing.PermitLimit = form.PermitLimit;
        existing.WindowSeconds = form.WindowSeconds;
        existing.QueueLimit = form.QueueLimit;
        existing.PartitionType = form.PartitionType;

        await _aiAdminManager.SaveRateLimitConfigAsync(existing);
        return RedirectToAction("RateLimits");
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("admin/language")]
    public IActionResult SetLanguage(string lang, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(lang))
        {
            lang = "en";
        }

        Response.Cookies.Append(
            ".Conversey.Admin.Culture",
            $"c={lang}-{(lang == "nl" ? "BE" : lang == "fr" ? "BE" : "US")}|uic={lang}-{(lang == "nl" ? "BE" : lang == "fr" ? "BE" : "US")}",
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), HttpOnly = true, SameSite = SameSiteMode.Lax }
        );

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index");
    }
}
