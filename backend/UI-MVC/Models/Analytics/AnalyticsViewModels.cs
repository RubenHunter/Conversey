using System.ComponentModel.DataAnnotations;
using Conversey.BL.Analytics.DTOs;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;

namespace Conversey.UI_MVC.Models.Analytics;

public class AnalyticsFilterViewModel
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? TopicId { get; set; }
    public string? Status { get; set; }
    public List<TopicOption> AvailableTopics { get; set; } = new();
    public List<string> AvailableStatuses { get; set; } = new() { "Pending", "Approved", "Rejected" };
}

public class TopicOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ProjectParticipantSummary
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectSlug { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public int IdeaCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ToxicityCount
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class WorkspaceAnalyticsViewModel
{
    public Workspace Workspace { get; set; } = null!;
    public Project? SelectedProject { get; set; }
    public Slug? ProjectId { get; set; }
    public List<Project> AvailableProjects { get; set; } = new();
    public List<ProjectParticipantSummary> ProjectParticipants { get; set; } = new();
    public int TotalWorkspaceParticipants { get; set; }
    public double EmailPercentage { get; set; }
    public List<ToxicityCount> ToxicityStats { get; set; } = new();
    public List<ToxicityCount> ResponseToxicityStats { get; set; } = new();
    public int DistinctFlaggedIdeas { get; set; }
    public int DistinctFlaggedResponses { get; set; }
    public int TotalComments { get; set; }
    public AnalyticsFilterViewModel Filter { get; set; } = new();
    public AnalyticsDashboardDto Dashboard { get; set; } = new();
    public AiSummaryResponseDto? AiSummary { get; set; }
    public string DashboardJson { get; set; } = "{}";
    public string ProjectCirclesJson { get; set; } = "[]";
    public string UsageTrendJson { get; set; } = "[]";
}

public class IdeasListViewModel
{
    public List<IdeaStatDto> Ideas { get; set; } = new();
    public Dictionary<int, List<CommentItemViewModel>> Comments { get; set; } = new();
    public List<Project> AvailableProjects { get; set; } = new();
    public List<TopicOption> AvailableTopics { get; set; } = new();
    public List<YouthOption> AvailableYouth { get; set; } = new();
    public List<string> AvailableCategories { get; set; } = new();
    public string? SelectedProjectId { get; set; }
    public string? TopicId { get; set; }
    public string? YouthId { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class AnswersListViewModel
{
    public List<AnswerListItemDto> OpenAnswers { get; set; } = new();
    public List<Project> AvailableProjects { get; set; } = new();
    public List<YouthOption> AvailableYouth { get; set; } = new();
    public List<string> AvailableQuestionTypes { get; set; } = new();
    public string? SelectedProjectId { get; set; }
    public string? YouthId { get; set; }
    public string? QuestionType { get; set; }
    public string? QuestionSearch { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class YouthOption
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string Display => Email ?? $"(Anonymous {Id.ToString()[..8]})";
}

public class ConverseyAnalyticsViewModel
{
    public List<PlatformWorkspaceStatDto> PlatformStats { get; set; } = new();
    public List<PlatformWorkspaceStatDto> AllPlatformStats { get; set; } = new();
    public AnalyticsFilterViewModel Filter { get; set; } = new();
    public AnalyticsDashboardDto? SelectedWorkspaceDashboard { get; set; }
    public AiSummaryResponseDto? AiSummary { get; set; }
    public PlatformModerationStatsDto? ModerationStats { get; set; }
    public PlatformUserStatsDto? UserStats { get; set; }
    public string? SelectedWorkspaceId { get; set; }
    public string DashboardJson { get; set; } = "{}";
    public string PlatformStatsJson { get; set; } = "[]";
    public string ModerationStatsJson { get; set; } = "{}";
    public string UserStatsJson { get; set; } = "{}";
    public string UsageTrendJson { get; set; } = "[]";
    public string WorkspaceCirclesJson { get; set; } = "[]";
}

public class ExportRequestViewModel
{
    public string ExportType { get; set; } = "quantitative";
    [Range(1, 3)]
    public int Format { get; set; } = 1;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? TopicId { get; set; }
    public string? Status { get; set; }
}

public class CommentItemViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? YouthId { get; set; }
    public string? YouthEmail { get; set; }
    public bool IsAuthor { get; set; }
    public bool MarkedForReview { get; set; }
}
