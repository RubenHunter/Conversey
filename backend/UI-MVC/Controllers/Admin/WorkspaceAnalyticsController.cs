using System.Globalization;
using System.Text.Json;
using Conversey.BL.Administration;
using Conversey.BL.Analytics;
using Conversey.BL.Analytics.DTOs;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.DAL.Analytics;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Models.Analytics;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Admin;

[Authorize(Policy = WorkspaceAdminPolicy.Name)]
public class WorkspaceAnalyticsController : Controller
{
    private readonly WorkspaceContext _workspaceContext;
    private readonly IAnalyticsManager _analyticsManager;
    private readonly IProjectManager _projectManager;
    private readonly IAnalyticsRepository _analyticsRepo;

    public WorkspaceAnalyticsController(
        WorkspaceContext workspaceContext,
        IAnalyticsManager analyticsManager,
        IProjectManager projectManager,
        IAnalyticsRepository analyticsRepo)
    {
        _workspaceContext = workspaceContext;
        _analyticsManager = analyticsManager;
        _projectManager = projectManager;
        _analyticsRepo = analyticsRepo;
    }

    [HttpGet("/admin/workspace/analytics")]
    public async Task<IActionResult> Index(
        [FromQuery] string? projectId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int? topicId = null,
        [FromQuery] string? status = null)
    {
        var workspace = _workspaceContext.CurrentWorkspace;
        ViewData["WorkspaceName"] = workspace.Name;
        DateTime? parsedDateFrom = ParseDate(Request.Query["dateFrom"]);
        DateTime? parsedDateTo = ParseDate(Request.Query["dateTo"]);
        var projects = _projectManager.GetAllProjectsFromWorkspaceId(workspace.Id);
        Project? selectedProject = null;

        if (!string.IsNullOrWhiteSpace(projectId))
        {
            selectedProject = projects.FirstOrDefault(p => p.Id.Text == projectId);
        }

        var filter = new AnalyticsFilterViewModel
        {
            DateFrom = parsedDateFrom,
            DateTo = parsedDateTo,
            TopicId = topicId,
            Status = status
        };

        if (selectedProject != null)
        {
            filter.AvailableTopics = selectedProject.Topic?
                .Select(t => new TopicOption { Id = t.Id, Name = t.Name })
                .ToList() ?? new List<TopicOption>();
        }
        else
        {
            filter.AvailableTopics = _analyticsRepo.GetTopicsForWorkspace(workspace.Id)
                .Select(t => new TopicOption { Id = t.Id, Name = t.Name })
                .DistinctBy(t => t.Id)
                .ToList();
        }

        Slug? projectSlug = selectedProject != null ? selectedProject.Id : null;

        var filters = new AnalyticsFilterRequest
        {
            DateFrom = parsedDateFrom,
            DateTo = parsedDateTo,
            TopicId = topicId,
            Status = status
        };

        var dashboard = await _analyticsManager.GetDashboardAsync(workspace.Id, projectSlug, filters);

        var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var projectParticipants = new List<ProjectParticipantSummary>();
        int totalWorkspaceParticipants = 0;
        var toxicityStats = new List<Models.Analytics.ToxicityCount>();

        if (selectedProject == null)
        {
            var hasFilters = !string.IsNullOrWhiteSpace(status) || topicId.HasValue || parsedDateFrom.HasValue || parsedDateTo.HasValue;
            var partFilters = hasFilters ? new AnalyticsFilterParams
            {
                DateFrom = parsedDateFrom,
                DateTo = parsedDateTo,
                TopicId = topicId,
                Status = status
            } : null;

            foreach (var p in projects)
            {
                var pStats = _analyticsRepo.GetParticipationStats(workspace.Id, p.Id, partFilters);
                projectParticipants.Add(new ProjectParticipantSummary
                {
                    ProjectName = p.Name,
                    ProjectSlug = p.Id.Text,
                    ParticipantCount = pStats.TotalYouth,
                    IdeaCount = pStats.YouthWithIdeas,
                    Status = p.Status.ToString()
                });
                totalWorkspaceParticipants += pStats.TotalYouth;
            }
        }

        foreach (var t in _analyticsRepo.GetToxicityStats(workspace.Id, projectSlug, new AnalyticsFilterParams
        {
            DateFrom = parsedDateFrom,
            DateTo = parsedDateTo,
            TopicId = topicId,
            Status = status
        }))
        {
            toxicityStats.Add(new Models.Analytics.ToxicityCount { Label = t.Label, Count = t.Count });
        }

        var responseToxicityStats = new List<Models.Analytics.ToxicityCount>();
        foreach (var t in _analyticsRepo.GetResponseToxicityStats(workspace.Id, projectSlug, new AnalyticsFilterParams
        {
            DateFrom = parsedDateFrom,
            DateTo = parsedDateTo,
            TopicId = topicId,
            Status = status
        }))
        {
            responseToxicityStats.Add(new Models.Analytics.ToxicityCount { Label = t.Label, Count = t.Count });
        }

        var totalComments = _analyticsRepo.GetTotalComments(workspace.Id, projectSlug);

        var distinctFlaggedIdeas = _analyticsRepo.GetDistinctFlaggedIdeaCount(workspace.Id, projectSlug, new AnalyticsFilterParams
        {
            DateFrom = parsedDateFrom,
            DateTo = parsedDateTo,
            TopicId = topicId,
            Status = status
        });
        var distinctFlaggedResponses = _analyticsRepo.GetDistinctFlaggedResponseCount(workspace.Id, projectSlug, new AnalyticsFilterParams
        {
            DateFrom = parsedDateFrom,
            DateTo = parsedDateTo,
            TopicId = topicId,
            Status = status
        });

        var emailPct = _analyticsRepo.GetEmailPercentage(workspace.Id, projectSlug);

        var cachedSummary = await _analyticsManager.GetCachedSummaryAsync(workspace.Id, projectSlug);

        var trend = new List<UsageTrendPointDto>();
        try
        {
            trend = _analyticsManager.GetUsageTrend(workspace.Id, projectSlug, parsedDateFrom, parsedDateTo);
        }
        catch { }

        var vm = new WorkspaceAnalyticsViewModel
        {
            Workspace = workspace,
            SelectedProject = selectedProject,
            ProjectId = projectSlug,
            AvailableProjects = projects.ToList(),
            ProjectParticipants = projectParticipants,
            TotalWorkspaceParticipants = totalWorkspaceParticipants,
            EmailPercentage = emailPct,
            ToxicityStats = toxicityStats,
            ResponseToxicityStats = responseToxicityStats,
            DistinctFlaggedIdeas = distinctFlaggedIdeas,
            DistinctFlaggedResponses = distinctFlaggedResponses,
            TotalComments = totalComments,
            Filter = filter,
            Dashboard = dashboard,
            AiSummary = cachedSummary,
            DashboardJson = JsonSerializer.Serialize(dashboard, jsonOpts),
            ProjectCirclesJson = JsonSerializer.Serialize(projectParticipants, jsonOpts),
            UsageTrendJson = JsonSerializer.Serialize(trend, jsonOpts)
        };

        return View("~/Views/WorkspaceAdmin/Analytics/Index.cshtml", vm);
    }

    [HttpGet("/admin/workspace/analytics/ideas")]
    public IActionResult IdeasList(
        [FromQuery] string? projectId = null,
        [FromQuery] string? topicId = null,
        [FromQuery] string? youthId = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var workspace = _workspaceContext.CurrentWorkspace;
        var projects = _projectManager.GetAllProjectsFromWorkspaceId(workspace.Id);

        Slug? projectSlug = null;
        if (!string.IsNullOrWhiteSpace(projectId))
            projectSlug = new Slug { Text = projectId };

        var filters = new AnalyticsFilterRequest
        {
            TopicId = int.TryParse(topicId, out var tid) ? tid : null,
            Status = status,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var ideas = _analyticsManager.GetIdeaStats(workspace.Id, projectSlug, filters);

        Guid? parsedYouthId = null;
        if (!string.IsNullOrWhiteSpace(youthId) && Guid.TryParse(youthId, out var yid))
            parsedYouthId = yid;

        if (parsedYouthId.HasValue)
        {
            ideas = ideas.Where(i => i.YouthId == parsedYouthId.Value).ToList();

            var projectIds = new HashSet<Slug>();
            if (projectSlug.HasValue)
                projectIds.Add(projectSlug.Value);
            else
                projectIds = new HashSet<Slug>(projects.Select(p => p.Id));

            var youthCommentedIdeaIds = _analyticsRepo.GetIdeaIdsCommentedByYouth(parsedYouthId.Value, projectIds);

            if (youthCommentedIdeaIds.Count > 0)
            {
                var existingIds = new HashSet<int>(ideas.Select(i => i.Id));
                var newIdeaIds = youthCommentedIdeaIds.Where(id => !existingIds.Contains(id)).ToHashSet();
                if (newIdeaIds.Count > 0)
                {
                    var allIdeas = _analyticsManager.GetIdeaStats(workspace.Id, projectSlug, null);
                    var extraIdeas = allIdeas.Where(i => newIdeaIds.Contains(i.Id)).ToList();
                    ideas = ideas.Concat(extraIdeas).ToList();
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(category))
            ideas = ideas.Where(i => i.SemanticCategories.Contains(category, StringComparer.OrdinalIgnoreCase)).ToList();

        var ideaIds = new HashSet<int>(ideas.Select(i => i.Id));
        var allResponses = _analyticsRepo.GetResponsesForIdeas(ideaIds);

        var comments = new Dictionary<int, List<CommentItemViewModel>>();
        foreach (var idea in ideas)
        {
            var ideaComments = allResponses
                .Where(r => r.Idea != null && r.Idea.Id == idea.Id)
                .OrderBy(r => idea.YouthId.HasValue && r.Youth.Id == idea.YouthId.Value ? 0 : 1)
                .ThenBy(r => r.CreatedAt)
                .Select(r => new CommentItemViewModel
                {
                    Id = r.Id,
                    Text = r.Text,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status.ToString(),
                    YouthId = r.Youth.Id,
                    YouthEmail = r.Youth.Email,
                    IsAuthor = idea.YouthId.HasValue && r.Youth.Id == idea.YouthId.Value,
                    MarkedForReview = r.MarkedForReview,
                    RejectionReason = r.RejectionReason
                })
                .ToList();
            comments[idea.Id] = ideaComments;
        }

        var topics = _analyticsRepo.GetTopicsForWorkspace(workspace.Id);
        if (projectSlug.HasValue)
            topics = topics.Where(t => t.Project.Id == projectSlug.Value).ToList();

        var youthList = _analyticsRepo.GetYouthList(workspace.Id, projectSlug);
        var categories = _analyticsRepo.GetDistinctCategories(workspace.Id, projectSlug);

        var vm = new IdeasListViewModel
        {
            Ideas = ideas,
            Comments = comments,
            AvailableProjects = projects.ToList(),
            AvailableTopics = topics.Select(t => new TopicOption { Id = t.Id, Name = t.Name }).DistinctBy(x => x.Id).ToList(),
            AvailableYouth = youthList.Select(y => new YouthOption { Id = y.Id, Email = y.Email }).ToList(),
            AvailableCategories = categories.ToList(),
            SelectedProjectId = projectId,
            TopicId = topicId,
            YouthId = youthId,
            Category = category,
            Status = status,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        return View("~/Views/WorkspaceAdmin/Analytics/IdeasList.cshtml", vm);
    }

    [HttpGet("/admin/workspace/analytics/answers")]
    public IActionResult AnswersList(
        [FromQuery] string? projectId = null,
        [FromQuery] string? youthId = null,
        [FromQuery] string? questionType = null,
        [FromQuery] string? questionSearch = null,
        [FromQuery] string? youthSearch = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var workspace = _workspaceContext.CurrentWorkspace;
        var projects = _projectManager.GetAllProjectsFromWorkspaceId(workspace.Id);

        Slug? projectSlug = null;
        if (!string.IsNullOrWhiteSpace(projectId))
            projectSlug = new Slug { Text = projectId };

        var filters = new AnalyticsFilterRequest { DateFrom = dateFrom, DateTo = dateTo };

        var allAnswers = _analyticsManager.GetAllAnswers(workspace.Id, projectSlug, filters);

        if (!string.IsNullOrWhiteSpace(youthId) && Guid.TryParse(youthId, out var yid))
            allAnswers = allAnswers.Where(a => a.YouthId == yid).ToList();

        if (!string.IsNullOrWhiteSpace(questionType))
            allAnswers = allAnswers.Where(a => string.Equals(a.QuestionType, questionType, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(questionSearch))
            allAnswers = allAnswers.Where(a => a.QuestionText.Contains(questionSearch, StringComparison.OrdinalIgnoreCase) || a.Value.Contains(questionSearch, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(youthSearch))
            allAnswers = allAnswers.Where(a => (a.YouthEmail != null && a.YouthEmail.Contains(youthSearch, StringComparison.OrdinalIgnoreCase)) || a.YouthId.ToString().Contains(youthSearch, StringComparison.OrdinalIgnoreCase)).ToList();

        allAnswers = allAnswers.OrderBy(a => a.YouthEmail ?? "").ThenBy(a => a.QuestionText).ToList();

        var youthList = _analyticsRepo.GetYouthList(workspace.Id, projectSlug);
        var questionTypes = _analyticsRepo.GetDistinctQuestionTypes(workspace.Id, projectSlug);

        var vm = new AnswersListViewModel
        {
            OpenAnswers = allAnswers,
            AvailableProjects = projects.ToList(),
            AvailableYouth = youthList.Select(y => new YouthOption { Id = y.Id, Email = y.Email }).ToList(),
            AvailableQuestionTypes = questionTypes.ToList(),
            SelectedProjectId = projectId,
            YouthId = youthId,
            QuestionType = questionType,
            QuestionSearch = questionSearch,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        return View("~/Views/WorkspaceAdmin/Analytics/AnswersList.cshtml", vm);
    }

    [HttpGet("/admin/workspace/moderation")]
    public IActionResult Moderation(
        [FromQuery] string? projectId = null,
        [FromQuery] string? topicId = null,
        [FromQuery] string? ideaId = null)
    {
        var workspace = _workspaceContext.CurrentWorkspace;
        ViewData["WorkspaceName"] = workspace.Name;
        var projects = _projectManager.GetAllProjectsFromWorkspaceId(workspace.Id);

        Slug? projectSlug = null;
        if (!string.IsNullOrWhiteSpace(projectId))
            projectSlug = new Slug { Text = projectId };

        int? parsedTopicId = null;
        if (!string.IsNullOrWhiteSpace(topicId) && int.TryParse(topicId, out var tid))
            parsedTopicId = tid;

        int? parsedIdeaId = null;
        if (!string.IsNullOrWhiteSpace(ideaId) && int.TryParse(ideaId, out var iid))
            parsedIdeaId = iid;

        var queue = _analyticsManager.GetModerationQueue(workspace.Id, projectSlug, parsedTopicId, parsedIdeaId);

        var topics = _analyticsRepo.GetTopicsForWorkspace(workspace.Id);
        if (projectSlug.HasValue)
            topics = topics.Where(t => t.Project.Id == projectSlug.Value).ToList();

        var vm = new ModerationViewModel
        {
            Items = queue.Select(q => new ModerationItemViewModel
            {
                Type = q.Type,
                Id = q.Id,
                Content = q.Content,
                SubmissionDate = q.SubmissionDate,
                TopicName = q.TopicName,
                ProjectName = q.ProjectName,
                ProjectSlug = q.ProjectSlug,
                TopicId = q.TopicId,
                ParentIdeaId = q.ParentIdeaId,
                ParentIdeaContent = q.ParentIdeaContent,
                YouthId = q.YouthId,
                YouthEmail = q.YouthEmail,
                FlagSexual = q.FlagSexual,
                FlagHate = q.FlagHate,
                FlagViolence = q.FlagViolence,
                FlagDangerous = q.FlagDangerous,
                FlagSelfHarm = q.FlagSelfHarm,
                FlagPii = q.FlagPii,
                RejectionReason = q.RejectionReason
            }).ToList(),
            AvailableProjects = projects.ToList(),
            AvailableTopics = topics.Select(t => new TopicOption { Id = t.Id, Name = t.Name }).DistinctBy(x => x.Id).ToList(),
            SelectedProjectId = projectId,
            TopicId = topicId,
            IdeaId = ideaId,
            TotalCount = queue.Count
        };

        return View("~/Views/WorkspaceAdmin/Analytics/Moderation.cshtml", vm);
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;
        return null;
    }
}
