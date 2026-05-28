#nullable enable
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;

namespace Conversey.DAL.Analytics;

public class AnalyticsFilterParams
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? TopicId { get; set; }
    public string? Status { get; set; }
}

public class QuestionChoiceStat
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public int? ChoiceId { get; set; }
    public string? ChoiceText { get; set; }
    public int Count { get; set; }
}

public class ScaleStat
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int LowerBound { get; set; }
    public int UpperBound { get; set; }
    public double Average { get; set; }
    public int Count { get; set; }
    public Dictionary<int, int> Distribution { get; set; } = new();
}

public class OpenAnswerItem
{
    public int AnswerId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid? YouthId { get; set; }
    public string? YouthEmail { get; set; }
}

public class IdeaStatItem
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string? TopicName { get; set; }
    public string[] SemanticCategories { get; set; } = Array.Empty<string>();
    public Guid? YouthId { get; set; }
    public string? YouthEmail { get; set; }
    public bool MarkedForReview { get; set; }
    public string? RejectionReason { get; set; }
}

public class IdeaCountByTopic
{
    public string TopicName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class IdeaCountByStatus
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class IdeaCountByCategory
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ParticipationStats
{
    public int TotalYouth { get; set; }
    public int YouthWithAnswers { get; set; }
    public int YouthWithIdeas { get; set; }
    public int YouthWithBoth { get; set; }
    public double ConversionRate { get; set; }
    public double AvgAnswersPerYouth { get; set; }
    public double AvgIdeasPerYouth { get; set; }
}

public class PlatformWorkspaceStat
{
    public string WorkspaceSlug { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public int YouthCount { get; set; }
    public int IdeaCount { get; set; }
    public int AnswerCount { get; set; }
    public double ConversionRate { get; set; }
}

public interface IAnalyticsRepository
{
    IReadOnlyCollection<QuestionChoiceStat> GetChoiceQuestionStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters);
    IReadOnlyCollection<ScaleStat> GetScaleQuestionStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters);
    IReadOnlyCollection<OpenAnswerItem> GetOpenAnswers(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters);
    IReadOnlyCollection<IdeaStatItem> GetIdeaStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters);
    IReadOnlyCollection<IdeaCountByTopic> GetIdeaCountByTopic(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters);
    IReadOnlyCollection<IdeaCountByStatus> GetIdeaCountByStatus(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters);
    IReadOnlyCollection<IdeaCountByCategory> GetIdeaCountByCategory(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters);
    ParticipationStats GetParticipationStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null);
    IReadOnlyCollection<PlatformWorkspaceStat> GetPlatformStats(Slug? workspaceId = null);
    PlatformModerationStats GetPlatformModerationStats(Slug? workspaceId = null);
    PlatformUserStats GetPlatformUserStats(Slug? workspaceId = null);
    IReadOnlyCollection<UsageTrendPoint> GetUsageTrend(Slug? workspaceId = null, Slug? projectId = null, DateTime? from = null, DateTime? to = null);
    IReadOnlyCollection<string> GetIdeaContentsForSummary(Slug workspaceId, Slug? projectId, int maxIdeas, AnalyticsFilterParams? filters);
    IReadOnlyCollection<Topic> GetTopicsForWorkspace(Slug workspaceId);
    IReadOnlyList<ToxicityCount> GetToxicityStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null);
    IReadOnlyList<ToxicityCount> GetResponseToxicityStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null);
    int GetDistinctFlaggedIdeaCount(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null);
    int GetDistinctFlaggedResponseCount(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null);
    int GetTotalComments(Slug workspaceId, Slug? projectId);
    double GetEmailPercentage(Slug workspaceId, Slug? projectId);
    IReadOnlyCollection<Youth> GetYouthList(Slug workspaceId, Slug? projectId);
    IReadOnlyCollection<string> GetDistinctCategories(Slug workspaceId, Slug? projectId);
    IReadOnlyCollection<string> GetDistinctQuestionTypes(Slug workspaceId, Slug? projectId);
    IReadOnlyCollection<AnswerListItem> GetAllAnswerItems(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters);
    IReadOnlyCollection<IdeaResponse> GetResponsesForIdeas(HashSet<int> ideaIds);
    HashSet<int> GetIdeaIdsCommentedByYouth(Guid youthId, HashSet<Slug> projectIds);
    Task<bool> ToggleMarkedForReviewAsync(string type, int id);
    Task<bool> SetModerationStatusAsync(string type, int id, string status, string? reason = null);
    IReadOnlyCollection<ModerationQueueItem> GetModerationQueue(Slug workspaceId, Slug? projectId, int? topicId, int? ideaId);
    Task<SavedAiSummary?> GetSavedSummaryAsync(Slug workspaceId, Slug? projectId);
    Task SaveSummaryAsync(SavedAiSummary summary);
}

public class ModerationQueueItem
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string? TopicName { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectSlug { get; set; }
    public int? TopicId { get; set; }
    public int? ParentIdeaId { get; set; }
    public string? ParentIdeaContent { get; set; }
    public Guid? YouthId { get; set; }
    public string? YouthEmail { get; set; }
    public bool FlagSexual { get; set; }
    public bool FlagHate { get; set; }
    public bool FlagViolence { get; set; }
    public bool FlagDangerous { get; set; }
    public bool FlagSelfHarm { get; set; }
    public bool FlagPii { get; set; }
    public string? RejectionReason { get; set; }
}

public class ToxicityCount
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AnswerListItem
{
    public int AnswerId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid? YouthId { get; set; }
    public string? YouthEmail { get; set; }
    public string ProjectName { get; set; } = string.Empty;
}

public class PlatformModerationStats
{
    public int TotalFlaggedIdeas { get; set; }
    public int TotalFlaggedComments { get; set; }
    public int TotalIdeas { get; set; }
    public int TotalComments { get; set; }
    public List<ToxicityCount> IdeaFlags { get; set; } = new();
    public List<ToxicityCount> CommentFlags { get; set; } = new();
}

public class PlatformUserStats
{
    public int TotalYouth { get; set; }
    public int YouthWithIdeas { get; set; }
    public int YouthWithAnswers { get; set; }
    public int YouthWithBoth { get; set; }
    public double AvgAnswersPerYouth { get; set; }
    public double AvgIdeasPerYouth { get; set; }
    public double ConversionRate { get; set; }
}

public class UsageTrendPoint
{
    public DateTime Date { get; set; }
    public int IdeaCount { get; set; }
    public int UniqueYouth { get; set; }
}
