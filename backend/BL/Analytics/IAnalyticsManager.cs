using Conversey.BL.Analytics.Dto;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Analytics;

public interface IAnalyticsManager
{
    Task<AnalyticsDashboardDto> GetDashboardAsync(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    List<ChoiceQuestionStatDto> GetChoiceQuestionStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    List<ScaleQuestionStatDto> GetScaleQuestionStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    List<OpenAnswerDto> GetOpenAnswers(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    List<AnswerListItemDto> GetAllAnswers(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    List<IdeaStatDto> GetIdeaStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    List<IdeaCountDto> GetIdeasByTopic(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    List<IdeaCountDto> GetIdeasByStatus(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    List<IdeaCountDto> GetIdeasByCategory(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    ParticipationStatsDto GetParticipationStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null);
    List<PlatformWorkspaceStatDto> GetPlatformStats(Slug? workspaceId = null);
    PlatformModerationStatsDto GetPlatformModerationStats(Slug? workspaceId = null);
    PlatformUserStatsDto GetPlatformUserStats(Slug? workspaceId = null);
    List<UsageTrendPointDto> GetUsageTrend(Slug? workspaceId = null, Slug? projectId = null, DateTime? from = null, DateTime? to = null);
    Task<AiSummaryResponseDto> GenerateIdeaSummaryAsync(Slug workspaceId, Slug? projectId, AiSummaryRequestDto request, AnalyticsFilterRequest filters);
    Task<AiSummaryResponseDto> GetCachedSummaryAsync(Slug workspaceId, Slug? projectId);
    Task SaveSummaryAsync(Slug workspaceId, Slug? projectId, AiSummaryRequestDto request, AiSummaryResponseDto response);
    string ExportQuantitativeCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters);
    string ExportQualitativeCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters, Guid? youthId = null, string category = null, string questionType = null);
    string ExportAnswersOnlyCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters, Guid? youthId = null, string questionType = null);
    string ExportIdeasOnlyCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters, Guid? youthId = null, string category = null);
    string ExportCombinedCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters, Guid? youthId = null, string category = null, string questionType = null);
    List<ModerationQueueItemDto> GetModerationQueue(Slug workspaceId, Slug? projectId, int? topicId, int? ideaId);
    Task<bool> SetModerationStatusAsync(string type, int id, string status, string reason = null);
    Task<bool> ToggleMarkedForReviewAsync(string type, int id);

    IReadOnlyCollection<Topic> GetTopicsForWorkspace(Slug workspaceId);
    IReadOnlyList<IdeaCountDto> GetToxicityStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null);
    IReadOnlyList<IdeaCountDto> GetResponseToxicityStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null);
    int GetDistinctFlaggedIdeaCount(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null);
    int GetDistinctFlaggedResponseCount(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null);
    int GetTotalComments(Slug workspaceId, Slug? projectId);
    double GetEmailPercentage(Slug workspaceId, Slug? projectId);
    IReadOnlyCollection<Youth> GetYouthList(Slug workspaceId, Slug? projectId);
    IReadOnlyCollection<string> GetDistinctCategories(Slug workspaceId, Slug? projectId);
    IReadOnlyCollection<string> GetDistinctQuestionTypes(Slug workspaceId, Slug? projectId);
    IReadOnlyCollection<IdeaResponse> GetResponsesForIdeas(HashSet<int> ideaIds);
    HashSet<int> GetIdeaIdsCommentedByYouth(Guid youthId, HashSet<Slug> projectIds);
}
