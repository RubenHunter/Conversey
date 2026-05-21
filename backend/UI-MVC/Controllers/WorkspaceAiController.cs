using Conversey.BL.Ai;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Models.AiAdmin;
using Conversey.UI_MVC.Models.WorkspaceAi;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers;

[Authorize(Policy = WorkspaceAdminPolicy.Name)]
public class WorkspaceAiController : Controller
{
    private readonly IAiAdminManager _aiAdminManager;
    private readonly IWorkspaceManager _workspaceManager;

    public WorkspaceAiController(IAiAdminManager aiAdminManager, IWorkspaceManager workspaceManager)
    {
        _aiAdminManager = aiAdminManager;
        _workspaceManager = workspaceManager;
    }

    private BL.Domain.Administration.Workspace GetWorkspace(string workspaceSlug)
    {
        return _workspaceManager.GetWorkspaceById(new Slug { Text = workspaceSlug });
    }

    [HttpGet]
    [Route("admin/{workspaceSlug}/ai")]
    public async Task<IActionResult> Dashboard(string workspaceSlug)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);
        var wsId = workspace.Id.ToString();

        var totalCost = await _aiAdminManager.GetWorkspaceTotalCostAsync(wsId, periodStart, periodEnd);
        var workspaceLimit = await _aiAdminManager.GetWorkspaceCostLimitAsync(wsId);
        var isOverLimit = await _aiAdminManager.IsWorkspaceOverLimitAsync(wsId);
        var costsPerProject = await _aiAdminManager.GetCostsPerProjectAsync(wsId, periodStart, periodEnd);

        var projects = workspace.Projects ?? new List<BL.Domain.Administration.Project>();
        var projectCosts = projects.Select(p =>
        {
            costsPerProject.TryGetValue(p.Id.ToString(), out var cost);
            return new ProjectCostEntry
            {
                ProjectId = p.Id.ToString(),
                ProjectName = p.Name,
                TotalCost = cost,
                CallCount = 0
            };
        }).OrderByDescending(p => p.TotalCost).ToList();

        var model = new WorkspaceAiDashboardViewModel
        {
            WorkspaceId = wsId,
            WorkspaceName = workspace.Name,
            WorkspaceTotalCost = totalCost,
            WorkspaceLimit = workspaceLimit,
            IsWorkspaceOverLimit = isOverLimit,
            ProjectCosts = projectCosts,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };

        ViewData["WorkspaceName"] = workspace.Name;
        ViewData["Breadcrumbs"] = new (string Label, string? Url, bool IsCurrent)[]
        {
            ("Dashboard", $"/admin/{workspaceSlug}", false),
            ("AI Settings", null, true)
        };

        return View(model);
    }

    [HttpGet]
    [Route("admin/{workspaceSlug}/ai/costs")]
    public async Task<IActionResult> Costs(string workspaceSlug, [FromQuery] WorkspaceAiCostsFilterViewModel filter)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var auditLogs = await _aiAdminManager.GetCostsFilteredAsync(
            workspaceId: workspace.Id.ToString(),
            projectId: filter?.ProjectId,
            modelName: filter?.ModelName,
            modelType: filter?.ModelType,
            providerName: filter?.ProviderName,
            promptName: filter?.PromptName,
            dateFrom: filter?.DateFrom.HasValue == true ? DateTime.SpecifyKind(filter.DateFrom.Value, DateTimeKind.Utc) : null,
            dateTo: filter?.DateTo.HasValue == true ? DateTime.SpecifyKind(filter.DateTo.Value, DateTimeKind.Utc) : null);

        var timeline = await _aiAdminManager.GetCostsTimelineAsync(
            days: filter?.TimelineDays ?? 30,
            workspaceId: workspace.Id.ToString(),
            projectId: filter?.ProjectId);

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

        var allLogs = await _aiAdminManager.GetCostsFilteredAsync(workspaceId: workspace.Id.ToString());
        var providers = await _aiAdminManager.GetAllProviderConfigsAsync();
        var prompts = await _aiAdminManager.GetAllPromptsAsync();

        var model = new WorkspaceAiCostsViewModel
        {
            WorkspaceId = workspace.Id.ToString(),
            Timeline = timeline,
            Summary = summary,
            AuditLogs = auditLogs,
            Projects = workspace.Projects?.ToList() ?? new List<BL.Domain.Administration.Project>(),
            Models = allLogs.Select(l => l.ModelName).Distinct().OrderBy(m => m).ToList(),
            ModelTypes = allLogs.Select(l => l.ModelType).Distinct().OrderBy(m => m).ToList(),
            Providers = providers.Select(p => p.ProviderName).Distinct().OrderBy(p => p).ToList(),
            Prompts = prompts.Select(p => p.Name).Distinct().OrderBy(p => p).ToList(),
            Filter = filter ?? new WorkspaceAiCostsFilterViewModel()
        };

        ViewData["WorkspaceName"] = workspace.Name;
        ViewData["Breadcrumbs"] = new (string Label, string? Url, bool IsCurrent)[]
        {
            ("Dashboard", $"/admin/{workspaceSlug}", false),
            ("AI Settings", $"/admin/{workspaceSlug}/ai", false),
            ("Costs & Audit", null, true)
        };

        return View("Costs/Costs", model);
    }

    [HttpGet]
    [Route("admin/{workspaceSlug}/ai/prompts")]
    public async Task<IActionResult> Prompts(string workspaceSlug, [FromQuery] string? search, [FromQuery] string? projectId)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var prompts = await _aiAdminManager.GetAllPromptsAsync();
        var workspacePrompts = prompts
            .Where(p => !p.Name.EndsWith("System", StringComparison.OrdinalIgnoreCase)
                        && !p.Name.StartsWith("Moderation", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var projects = workspace.Projects?.ToList() ?? new List<BL.Domain.Administration.Project>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            workspacePrompts = workspacePrompts
                .Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var model = new WorkspaceAiPromptsViewModel
        {
            WorkspaceId = workspace.Id.ToString(),
            Prompts = workspacePrompts,
            Projects = projects,
            SelectedProjectId = projectId ?? string.Empty,
            SearchQuery = search ?? string.Empty
        };

        ViewData["WorkspaceName"] = workspace.Name;
        ViewData["Breadcrumbs"] = new (string Label, string? Url, bool IsCurrent)[]
        {
            ("Dashboard", $"/admin/{workspaceSlug}", false),
            ("AI Settings", $"/admin/{workspaceSlug}/ai", false),
            ("Prompts", null, true)
        };

        return View("Prompts/Prompts", model);
    }

    [HttpGet]
    [Route("admin/{workspaceSlug}/ai/prompts/{id:int}/edit")]
    public async Task<IActionResult> EditPrompt(string workspaceSlug, int id, [FromQuery] string? projectId)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var prompt = await _aiAdminManager.GetPromptByIdAsync(id);
        if (prompt == null) return NotFound();

        var defaultPrompt = await _aiAdminManager.GetDefaultPromptAsync(prompt.Name);

        var model = new WorkspaceAiPromptEditViewModel
        {
            Prompt = prompt,
            DefaultPrompt = defaultPrompt ?? new AiPrompt(),
            HasDefault = defaultPrompt != null,
            WorkspaceId = workspace.Id.ToString(),
            ProjectId = projectId ?? string.Empty
        };

        ViewData["WorkspaceName"] = workspace.Name;
        ViewData["Breadcrumbs"] = new (string Label, string? Url, bool IsCurrent)[]
        {
            ("Dashboard", $"/admin/{workspaceSlug}", false),
            ("AI Settings", $"/admin/{workspaceSlug}/ai", false),
            ("Prompts", $"/admin/{workspaceSlug}/ai/prompts", false),
            (prompt.Name, null, true)
        };

        return View("Prompts/EditPrompt", model);
    }

    [HttpPost]
    [Route("admin/{workspaceSlug}/ai/prompts/{id:int}")]
    public async Task<IActionResult> SavePrompt(string workspaceSlug, int id, string UserPromptTemplate, string Description, [FromQuery] string? projectId)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var existing = await _aiAdminManager.GetPromptByIdAsync(id);
        if (existing == null) return NotFound();

        existing.UserPromptTemplate = UserPromptTemplate;
        existing.Description = Description;

        await _aiAdminManager.SavePromptAsync(existing);

        var redirectUrl = string.IsNullOrWhiteSpace(projectId)
            ? $"/admin/{workspaceSlug}/ai/prompts"
            : $"/admin/{workspaceSlug}/ai/prompts?projectId={projectId}";

        return Redirect(redirectUrl);
    }

    [HttpPost]
    [Route("admin/{workspaceSlug}/ai/prompts/{id:int}/reset")]
    public async Task<IActionResult> ResetPrompt(string workspaceSlug, int id, [FromQuery] string? projectId)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var prompt = await _aiAdminManager.GetPromptByIdAsync(id);
        if (prompt == null) return NotFound();

        var defaultPrompt = await _aiAdminManager.GetDefaultPromptAsync(prompt.Name);
        if (defaultPrompt == null) return BadRequest("No default available for this prompt.");

        prompt.UserPromptTemplate = defaultPrompt.UserPromptTemplate;
        prompt.Description = defaultPrompt.Description;

        await _aiAdminManager.SavePromptAsync(prompt);

        return RedirectToAction("EditPrompt", new { workspaceSlug, id, projectId });
    }

    [HttpGet]
    [Route("admin/{workspaceSlug}/ai/limits")]
    public async Task<IActionResult> Limits(string workspaceSlug)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var workspaceLimit = await _aiAdminManager.GetWorkspaceCostLimitAsync(workspace.Id.ToString());
        var workspaceLimitHistory = await _aiAdminManager.GetWorkspaceCostLimitsAsync(workspace.Id.ToString());
        var projects = workspace.Projects?.ToList() ?? new List<BL.Domain.Administration.Project>();

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        var projectLimits = new List<ProjectLimitEntry>();
        foreach (var project in projects)
        {
            var limit = await _aiAdminManager.GetProjectCostLimitAsync(project.Id.ToString());
            var currentCost = await _aiAdminManager.GetProjectTotalCostAsync(project.Id.ToString(), periodStart, periodEnd);
            var isOverLimit = limit != null && currentCost >= limit.LimitAmount;

            projectLimits.Add(new ProjectLimitEntry
            {
                ProjectId = project.Id.ToString(),
                ProjectName = project.Name,
                ActiveLimit = limit,
                CurrentCost = currentCost,
                IsOverLimit = isOverLimit
            });
        }

        var currentWorkspaceCost = await _aiAdminManager.GetWorkspaceTotalCostAsync(workspace.Id.ToString(), periodStart, periodEnd);

        var model = new WorkspaceAiLimitsViewModel
        {
            WorkspaceId = workspace.Id.ToString(),
            WorkspaceName = workspace.Name,
            WorkspaceLimit = workspaceLimit,
            WorkspaceLimitHistory = workspaceLimitHistory,
            ProjectLimits = projectLimits,
            CurrentWorkspaceCost = currentWorkspaceCost
        };

        ViewData["WorkspaceName"] = workspace.Name;
        ViewData["Breadcrumbs"] = new (string Label, string? Url, bool IsCurrent)[]
        {
            ("Dashboard", $"/admin/{workspaceSlug}", false),
            ("AI Settings", $"/admin/{workspaceSlug}/ai", false),
            ("Cost Limits", null, true)
        };

        return View("Limits/Limits", model);
    }

    [HttpPost]
    [Route("admin/{workspaceSlug}/ai/limits/workspace")]
    public async Task<IActionResult> SaveWorkspaceLimit(string workspaceSlug, AiCostLimitFormViewModel form)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        AiCostLimit limit;
        if (form.Id.HasValue && form.Id.Value > 0)
        {
            var limits = await _aiAdminManager.GetWorkspaceCostLimitsAsync(workspace.Id.ToString());
            limit = limits.FirstOrDefault(l => l.Id == form.Id.Value) ?? new AiCostLimit();
        }
        else
        {
            var existing = await _aiAdminManager.GetWorkspaceCostLimitAsync(workspace.Id.ToString());
            limit = existing ?? new AiCostLimit();
        }

        limit.LimitAmount = form.LimitAmount;
        limit.PeriodStart = form.PeriodStart;
        limit.PeriodEnd = form.PeriodEnd;
        limit.IsActive = form.IsActive;
        limit.WorkspaceId = workspace.Id;
        limit.ProjectId = null;

        await _aiAdminManager.SaveCostLimitAsync(limit);
        return RedirectToAction("Limits", new { workspaceSlug });
    }

    [HttpPost]
    [Route("admin/{workspaceSlug}/ai/limits/project/{projectId}")]
    public async Task<IActionResult> SaveProjectLimit(string workspaceSlug, string projectId, AiCostLimitFormViewModel form)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        AiCostLimit limit;
        if (form.Id.HasValue && form.Id.Value > 0)
        {
            var limits = await _aiAdminManager.GetProjectCostLimitsAsync(projectId);
            limit = limits.FirstOrDefault(l => l.Id == form.Id.Value) ?? new AiCostLimit();
        }
        else
        {
            var existing = await _aiAdminManager.GetProjectCostLimitAsync(projectId);
            limit = existing ?? new AiCostLimit();
        }

        limit.LimitAmount = form.LimitAmount;
        limit.PeriodStart = form.PeriodStart;
        limit.PeriodEnd = form.PeriodEnd;
        limit.IsActive = form.IsActive;
        limit.WorkspaceId = workspace.Id;
        limit.ProjectId = Slug.FromName(projectId);

        await _aiAdminManager.SaveCostLimitAsync(limit);
        return RedirectToAction("Limits", new { workspaceSlug });
    }

    [HttpPost]
    [Route("admin/{workspaceSlug}/ai/limits/{limitId:int}/delete")]
    public async Task<IActionResult> DeleteLimit(string workspaceSlug, int limitId)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        await _aiAdminManager.DeleteCostLimitAsync(limitId);
        return RedirectToAction("Limits", new { workspaceSlug });
    }

    [HttpGet]
    [Route("admin/{workspaceSlug}/ai/keywords")]
    public async Task<IActionResult> Keywords(string workspaceSlug)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var keywords = await _aiAdminManager.GetModerationKeywordsForWorkspaceAsync(workspaceSlug);
        ViewData["WorkspaceName"] = workspace.Name;
        ViewData["Breadcrumbs"] = new (string Label, string? Url, bool IsCurrent)[]
        {
            ("Dashboard", $"/admin/{workspaceSlug}", false),
            ("AI Settings", $"/admin/{workspaceSlug}/ai", false),
            ("Moderation Keywords", null, true)
        };
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
    [Route("admin/{workspaceSlug}/ai/keywords/create")]
    public async Task<IActionResult> CreateKeyword(string workspaceSlug, string keywordText)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var keyword = new ModerationKeyword
        {
            Keyword = keywordText,
            WorkspaceId = workspace.Id.ToString()
        };
        await _aiAdminManager.SaveModerationKeywordAsync(keyword);
        return RedirectToAction("Keywords", new { workspaceSlug });
    }

    [HttpPost]
    [Route("admin/{workspaceSlug}/ai/keywords/{id:int}/delete")]
    public async Task<IActionResult> DeleteKeyword(string workspaceSlug, int id)
    {
        var workspace = GetWorkspace(workspaceSlug);
        if (workspace == null) return NotFound();

        var keywords = await _aiAdminManager.GetModerationKeywordsForWorkspaceAsync(workspaceSlug);
        var kw = keywords.FirstOrDefault(k => k.Id == id);
        if (kw == null || kw.WorkspaceId != workspace.Id.ToString())
            return BadRequest("Cannot delete platform-wide keywords.");

        await _aiAdminManager.DeleteModerationKeywordAsync(id);
        return RedirectToAction("Keywords", new { workspaceSlug });
    }
}
