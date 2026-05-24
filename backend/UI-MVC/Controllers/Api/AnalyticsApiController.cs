using System.Text;
using Conversey.BL.Analytics;
using Conversey.BL.Analytics.DTOs;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Analytics;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/admin/analytics")]
[Authorize]
public class AnalyticsApiController : ControllerBase
{
    private readonly IAnalyticsManager _analyticsManager;
    private readonly IAnalyticsRepository _analyticsRepo;

    public AnalyticsApiController(IAnalyticsManager analyticsManager, IAnalyticsRepository analyticsRepo)
    {
        _analyticsManager = analyticsManager;
        _analyticsRepo = analyticsRepo;
    }

    private static Slug MakeSlug(string value) => new() { Text = value };

    private static Slug? MakeSlugOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : new Slug { Text = value };

    [HttpGet("dashboard")]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetDashboard(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? topicId = null,
        [FromQuery] string? status = null)
    {
        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            TopicId = topicId,
            Status = status
        };

        var dashboard = await _analyticsManager.GetDashboardAsync(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            filters);

        return Ok(dashboard);
    }

    [HttpGet("choice-stats")]
    public ActionResult<List<ChoiceQuestionStatDto>> GetChoiceStats(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? topicId = null,
        [FromQuery] string? status = null)
    {
        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            TopicId = topicId,
            Status = status
        };

        var stats = _analyticsManager.GetChoiceQuestionStats(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            filters);

        return Ok(stats);
    }

    [HttpGet("scale-stats")]
    public ActionResult<List<ScaleQuestionStatDto>> GetScaleStats(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
        };

        var stats = _analyticsManager.GetScaleQuestionStats(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            filters);

        return Ok(stats);
    }

    [HttpGet("ideas")]
    public ActionResult<List<IdeaStatDto>> GetIdeas(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? topicId = null,
        [FromQuery] string? status = null)
    {
        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            TopicId = topicId,
            Status = status
        };

        var ideas = _analyticsManager.GetIdeaStats(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            filters);

        return Ok(ideas);
    }

    [HttpGet("ideas-by-topic")]
    public ActionResult<List<IdeaCountDto>> GetIdeasByTopic(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var stats = _analyticsManager.GetIdeasByTopic(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            filters);

        return Ok(stats);
    }

    [HttpGet("ideas-by-status")]
    public ActionResult<List<IdeaCountDto>> GetIdeasByStatus(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var stats = _analyticsManager.GetIdeasByStatus(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            filters);

        return Ok(stats);
    }

    [HttpGet("ideas-by-category")]
    public ActionResult<List<IdeaCountDto>> GetIdeasByCategory(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var stats = _analyticsManager.GetIdeasByCategory(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            filters);

        return Ok(stats);
    }

    [HttpGet("participation")]
    public ActionResult<ParticipationStatsDto> GetParticipation(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null)
    {
        var stats = _analyticsManager.GetParticipationStats(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null);

        return Ok(stats);
    }

    [HttpGet("platform-stats")]
    [Authorize(Policy = ConverseyAdminPolicy.Name)]
    public ActionResult<List<PlatformWorkspaceStatDto>> GetPlatformStats()
    {
        var stats = _analyticsManager.GetPlatformStats();
        return Ok(stats);
    }

    [HttpPost("ai-summary")]
    public async Task<ActionResult<AiSummaryResponseDto>> GenerateAiSummary(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromBody] AiSummaryRequestDto? request = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? topicId = null,
        [FromQuery] string? status = null)
    {
        request ??= new AiSummaryRequestDto();

        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            TopicId = topicId,
            Status = status
        };

        var summary = await _analyticsManager.GenerateIdeaSummaryAsync(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            request,
            filters);

        summary.GeneratedAt = DateTime.UtcNow;

        await _analyticsManager.SaveSummaryAsync(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null,
            request,
            summary);

        return Ok(summary);
    }

    [HttpGet("ai-summary")]
    public async Task<ActionResult<AiSummaryResponseDto?>> GetCachedSummary(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null)
    {
        var summary = await _analyticsManager.GetCachedSummaryAsync(
            MakeSlug(workspaceId),
            projectId != null ? MakeSlug(projectId) : null);

        return Ok(summary);
    }

    [HttpPost("mark-for-review")]
    public async Task<IActionResult> ToggleMarkForReview([FromBody] MarkForReviewRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Type) || (request.Type != "idea" && request.Type != "response"))
            return BadRequest(new { error = "Type must be 'idea' or 'response'." });

        var result = await _analyticsRepo.ToggleMarkedForReviewAsync(request.Type, request.Id);
        if (!result)
            return NotFound(new { error = $"{request.Type} with id {request.Id} not found." });

        return Ok(new { success = true });
    }

    [HttpPost("moderate")]
    [Authorize(Policy = WorkspaceAdminPolicy.Name)]
    public async Task<IActionResult> Moderate([FromBody] ModerateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Type) || (request.Type != "idea" && request.Type != "response"))
            return BadRequest(new { error = "Type must be 'idea' or 'response'." });

        if (string.IsNullOrWhiteSpace(request.Action) || (request.Action != "accept" && request.Action != "deny"))
            return BadRequest(new { error = "Action must be 'accept' or 'deny'." });

        var status = request.Action == "accept" ? "Approved" : "Rejected";
        var result = await _analyticsRepo.SetModerationStatusAsync(request.Type, request.Id, status, request.Reason);
        if (!result)
            return NotFound(new { error = $"{request.Type} with id {request.Id} not found or invalid status." });

        return Ok(new { success = true, status });
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] string workspaceId,
        [FromQuery] string? projectId = null,
        [FromQuery] string type = "combined",
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? topicId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? youthId = null,
        [FromQuery] string? category = null,
        [FromQuery] string? questionType = null)
    {
        var filters = new AnalyticsFilterRequest
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            TopicId = topicId,
            Status = status
        };

        Guid? parsedYouthId = null;
        if (!string.IsNullOrWhiteSpace(youthId) && Guid.TryParse(youthId, out var yid))
            parsedYouthId = yid;

        string csv;
        var filenamePrefix = $"export_{workspaceId}";

        switch (type.ToLower())
        {
            case "quantitative":
                csv = _analyticsManager.ExportQuantitativeCsv(
                    MakeSlug(workspaceId),
                    projectId != null ? MakeSlug(projectId) : null,
                    filters);
                filenamePrefix += "_quantitative";
                break;
            case "qualitative":
                csv = _analyticsManager.ExportQualitativeCsv(
                    MakeSlug(workspaceId),
                    projectId != null ? MakeSlug(projectId) : null,
                    filters,
                    parsedYouthId,
                    category,
                    questionType);
                filenamePrefix += "_qualitative";
                break;
            case "answers-only":
                csv = _analyticsManager.ExportAnswersOnlyCsv(
                    MakeSlug(workspaceId),
                    projectId != null ? MakeSlug(projectId) : null,
                    filters,
                    parsedYouthId,
                    questionType);
                filenamePrefix += "_answers";
                break;
            case "ideas-only":
                csv = _analyticsManager.ExportIdeasOnlyCsv(
                    MakeSlug(workspaceId),
                    projectId != null ? MakeSlug(projectId) : null,
                    filters,
                    parsedYouthId,
                    category);
                filenamePrefix += "_ideas";
                break;
            default:
                csv = _analyticsManager.ExportCombinedCsv(
                    MakeSlug(workspaceId),
                    projectId != null ? MakeSlug(projectId) : null,
                    filters,
                    parsedYouthId,
                    category,
                    questionType);
                break;
        }

        var filename = $"{filenamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var bytes = Encoding.UTF8.GetBytes(csv);
        var bom = Encoding.UTF8.GetPreamble();
        var fullContent = bom.Concat(bytes).ToArray();

        return File(fullContent, "text/csv; charset=utf-8", filename);
    }

    [HttpGet("usage-trend")]
    public IActionResult GetUsageTrend(
        [FromQuery] string? workspaceId = null,
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        Slug? wsSlug = string.IsNullOrWhiteSpace(workspaceId) ? null : MakeSlugOrNull(workspaceId);
        Slug? pSlug = string.IsNullOrWhiteSpace(projectId) ? null : MakeSlugOrNull(projectId);
        var trend = _analyticsManager.GetUsageTrend(wsSlug, pSlug, from, to);
        return Ok(trend);
    }
}
