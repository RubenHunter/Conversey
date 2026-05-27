namespace Conversey.BL.Analytics.DTOs;

public class AnalyticsFilterRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? TopicId { get; set; }
    public string? Status { get; set; }
}

public class ChoiceQuestionStatDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public List<ChoiceCountDto> Choices { get; set; } = new();
}

public class ChoiceCountDto
{
    public int ChoiceId { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ScaleQuestionStatDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int LowerBound { get; set; }
    public int UpperBound { get; set; }
    public double Average { get; set; }
    public int Count { get; set; }
    public Dictionary<int, int> Distribution { get; set; } = new();
}

public class OpenAnswerDto
{
    public int AnswerId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid? YouthId { get; set; }
    public string? YouthEmail { get; set; }
}

public class AnswerListItemDto
{
    public int AnswerId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Guid? YouthId { get; set; }
    public string? YouthEmail { get; set; }
    public string ProjectName { get; set; } = string.Empty;
}

public class IdeaStatDto
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

public class IdeaCountDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ParticipationStatsDto
{
    public int TotalYouth { get; set; }
    public int YouthWithAnswers { get; set; }
    public int YouthWithIdeas { get; set; }
    public int YouthWithBoth { get; set; }
    public double ConversionRate { get; set; }
    public double AvgAnswersPerYouth { get; set; }
    public double AvgIdeasPerYouth { get; set; }
}

public class PlatformWorkspaceStatDto
{
    public string WorkspaceSlug { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public int YouthCount { get; set; }
    public int IdeaCount { get; set; }
    public int AnswerCount { get; set; }
    public double ConversionRate { get; set; }
}

public class AiSummaryResponseDto
{
    public string Overview { get; set; } = string.Empty;
    public List<string> Trends { get; set; } = new();
    public List<string> MinorityViews { get; set; } = new();
    public List<string> NotableQuotes { get; set; } = new();
    public List<string> SuggestedActions { get; set; } = new();
    public DateTime? GeneratedAt { get; set; }
}

public class AiSummaryRequestDto
{
    public string? Focus { get; set; }
    public string? Language { get; set; }
}

public class MarkForReviewRequest
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
}

public class ModerateRequest
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class AnalyticsDashboardDto
{
    public List<ChoiceQuestionStatDto> ChoiceQuestionStats { get; set; } = new();
    public List<ScaleQuestionStatDto> ScaleQuestionStats { get; set; } = new();
    public List<OpenAnswerDto> OpenAnswers { get; set; } = new();
    public List<IdeaStatDto> Ideas { get; set; } = new();
    public List<IdeaCountDto> IdeasByTopic { get; set; } = new();
    public List<IdeaCountDto> IdeasByStatus { get; set; } = new();
    public List<IdeaCountDto> IdeasByCategory { get; set; } = new();
    public ParticipationStatsDto Participation { get; set; } = new();
}

public class PlatformModerationStatsDto
{
    public int TotalFlaggedIdeas { get; set; }
    public int TotalFlaggedComments { get; set; }
    public int TotalIdeas { get; set; }
    public int TotalComments { get; set; }
    public List<IdeaCountDto> IdeaFlags { get; set; } = new();
    public List<IdeaCountDto> CommentFlags { get; set; } = new();
}

public class PlatformUserStatsDto
{
    public int TotalYouth { get; set; }
    public int YouthWithIdeas { get; set; }
    public int YouthWithAnswers { get; set; }
    public int YouthWithBoth { get; set; }
    public double AvgAnswersPerYouth { get; set; }
    public double AvgIdeasPerYouth { get; set; }
    public double ConversionRate { get; set; }
}

public class UsageTrendPointDto
{
    public string Date { get; set; } = string.Empty;
    public int IdeaCount { get; set; }
    public int UniqueYouth { get; set; }
}

public class ModerationQueueItemDto
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
