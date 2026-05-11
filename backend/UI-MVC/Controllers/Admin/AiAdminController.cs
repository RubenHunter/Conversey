using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize(Policy = ConverseyAdminPolicy.Name)]
[Route("admin/ai")]
public class AiAdminController : Controller
{
    private readonly IAiAdminManager _aiAdminManager;

    public AiAdminController(IAiAdminManager aiAdminManager)
    {
        _aiAdminManager = aiAdminManager;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("prompts")]
    public async Task<IActionResult> Prompts()
    {
        var prompts = await _aiAdminManager.GetAllPromptsAsync();
        return View(prompts);
    }

    [HttpGet("prompts/{id:int}")]
    public async Task<IActionResult> EditPrompt(int id)
    {
        var prompt = await _aiAdminManager.GetPromptByIdAsync(id);
        if (prompt == null)
        {
            return NotFound();
        }

        return View(prompt);
    }

    [HttpPost("prompts/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPrompt(int id, AiPrompt model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _aiAdminManager.GetPromptByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.SystemPrompt = model.SystemPrompt;
        existing.UserPromptTemplate = model.UserPromptTemplate;
        existing.Description = model.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        await _aiAdminManager.SavePromptAsync(existing);
        return RedirectToAction(nameof(Prompts));
    }

    [HttpGet("providers")]
    public async Task<IActionResult> Providers()
    {
        var configs = await _aiAdminManager.GetAllProviderConfigsAsync();
        return View(configs);
    }

    [HttpGet("providers/create")]
    public IActionResult CreateProvider()
    {
        return View("EditProvider", new AiProviderConfig { IsEnabled = false });
    }

    [HttpGet("providers/{id:int}")]
    public async Task<IActionResult> EditProvider(int id)
    {
        var config = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (config == null)
        {
            return NotFound();
        }

        if (config.Temperature == 0m)
        {
            config.Temperature = 1.0m;
        }

        return View(config);
    }

    [HttpPost("providers/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProvider(int id, AiProviderConfig model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.ProviderName = model.ProviderName;
        existing.BaseUrl = model.BaseUrl;
        existing.ApiKey = model.ApiKey;
        existing.ApiVersion = model.ApiVersion;
        existing.IsEnabled = model.IsEnabled;

        await _aiAdminManager.SaveProviderConfigAsync(existing);
        return RedirectToAction(nameof(ConfigureModels), new { id });
    }

    [HttpPost("providers/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProvider(AiProviderConfig model)
    {
        if (!ModelState.IsValid)
        {
            return View("EditProvider", model);
        }

        model.Id = 0;
        await _aiAdminManager.SaveProviderConfigAsync(model);
        return RedirectToAction(nameof(ConfigureModels), new { id = model.Id });
    }

    [HttpPost("providers/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProvider(int id)
    {
        var config = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (config == null)
        {
            return NotFound();
        }

        await _aiAdminManager.DeleteProviderConfigAsync(id);
        return RedirectToAction(nameof(Providers));
    }

    [HttpGet("costs")]
    public async Task<IActionResult> Costs()
    {
        var costs = await _aiAdminManager.GetAllCostsAsync();
        return View(costs);
    }

    [HttpGet("providers/{id:int}/models")]
    public async Task<IActionResult> ConfigureModels(int id)
    {
        var config = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (config == null)
        {
            return NotFound();
        }

        IReadOnlyList<string> models = Array.Empty<string>();
        string fetchError = null;

        try
        {
            models = await _aiAdminManager.ListProviderModelsAsync(id);
        }
        catch (Exception ex)
        {
            fetchError = ex.Message;
        }

        ViewBag.AvailableModels = models;
        ViewBag.FetchError = fetchError;
        return View(config);
    }

    [HttpPost("providers/{id:int}/models")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigureModels(int id, AiProviderConfig model)
    {
        var config = await _aiAdminManager.GetProviderConfigByIdAsync(id);
        if (config == null)
        {
            return NotFound();
        }

        config.CompletionsModel = model.CompletionsModel;
        config.ModerationModel = model.ModerationModel;
        config.Temperature = model.Temperature;

        await _aiAdminManager.SaveProviderConfigAsync(config);
        return RedirectToAction(nameof(Providers));
    }

    [HttpGet("rate-limits")]
    public async Task<IActionResult> RateLimits()
    {
        var configs = await _aiAdminManager.GetAllRateLimitConfigsAsync();
        return View(configs);
    }

    [HttpGet("rate-limits/{id:int}")]
    public async Task<IActionResult> EditRateLimit(int id)
    {
        var config = await _aiAdminManager.GetRateLimitConfigByIdAsync(id);
        if (config == null)
        {
            return NotFound();
        }

        return View(config);
    }

    [HttpPost("rate-limits/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRateLimit(int id, RateLimitConfig model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await _aiAdminManager.GetRateLimitConfigByIdAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.PermitLimit = model.PermitLimit;
        existing.WindowSeconds = model.WindowSeconds;
        existing.QueueLimit = model.QueueLimit;

        await _aiAdminManager.SaveRateLimitConfigAsync(existing);
        return RedirectToAction(nameof(RateLimits));
    }
}
