using System.Text;
using System.Text.Json;
using Conversey.BL.Ai;
using Conversey.BL.Analytics.Dto;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Analytics;
using Conversey.DAL.Subplatform.Ai;

namespace Conversey.BL.Analytics;

public class AnalyticsManager : IAnalyticsManager
{
    private readonly IAnalyticsRepository _repo;
    private readonly IAiManager _aiManager;
    private readonly IPromptRepository _promptRepository;

    public AnalyticsManager(IAnalyticsRepository repo, IAiManager aiManager, IPromptRepository promptRepository)
    {
        _repo = repo;
        _aiManager = aiManager;
        _promptRepository = promptRepository;
    }

    public async Task<AnalyticsDashboardDto> GetDashboardAsync(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        return new AnalyticsDashboardDto
        {
            ChoiceQuestionStats = GetChoiceQuestionStats(workspaceId, projectId, filters),
            ScaleQuestionStats = GetScaleQuestionStats(workspaceId, projectId, filters),
            OpenAnswers = GetOpenAnswers(workspaceId, projectId, filters),
            Ideas = GetIdeaStats(workspaceId, projectId, filters),
            IdeasByTopic = GetIdeasByTopic(workspaceId, projectId, filters),
            IdeasByStatus = GetIdeasByStatus(workspaceId, projectId, filters),
            IdeasByCategory = GetIdeasByCategory(workspaceId, projectId, filters),
            Participation = GetParticipationStats(workspaceId, projectId, filters)
        };
    }

    public List<ChoiceQuestionStatDto> GetChoiceQuestionStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var stats = _repo.GetChoiceQuestionStats(workspaceId, projectId, ToRepoFilters(filters));

        return stats
            .GroupBy(s => new { s.QuestionId, s.QuestionText, s.QuestionType })
            .Select(g => new ChoiceQuestionStatDto
            {
                QuestionId = g.Key.QuestionId,
                QuestionText = g.Key.QuestionText,
                QuestionType = g.Key.QuestionType,
                Choices = g.Select(c => new ChoiceCountDto
                {
                    ChoiceId = c.ChoiceId ?? 0,
                    ChoiceText = c.ChoiceText ?? string.Empty,
                    Count = c.Count
                }).OrderByDescending(c => c.Count).ToList()
            })
            .OrderBy(q => q.QuestionId)
            .ToList();
    }

    public List<ScaleQuestionStatDto> GetScaleQuestionStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var stats = _repo.GetScaleQuestionStats(workspaceId, projectId, ToRepoFilters(filters));

        return stats.Select(s => new ScaleQuestionStatDto
        {
            QuestionId = s.QuestionId,
            QuestionText = s.QuestionText,
            LowerBound = s.LowerBound,
            UpperBound = s.UpperBound,
            Average = s.Average,
            Count = s.Count,
            Distribution = s.Distribution
        }).ToList();
    }

    public List<OpenAnswerDto> GetOpenAnswers(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var answers = _repo.GetOpenAnswers(workspaceId, projectId, ToRepoFilters(filters));

        return answers.Select(a => new OpenAnswerDto
        {
            AnswerId = a.AnswerId,
            QuestionText = a.QuestionText,
            QuestionType = a.QuestionType,
            Value = a.Value,
            YouthId = a.YouthId,
            YouthEmail = a.YouthEmail
        }).ToList();
    }

    public List<AnswerListItemDto> GetAllAnswers(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var items = _repo.GetAllAnswerItems(workspaceId, projectId, ToRepoFilters(filters));

        var singleItems = items
            .Where(a => a.QuestionType != "MultipleChoice")
            .Select(a => new AnswerListItemDto
            {
                AnswerId = a.AnswerId,
                QuestionText = a.QuestionText,
                QuestionType = a.QuestionType,
                Value = a.Value,
                YouthId = a.YouthId,
                YouthEmail = a.YouthEmail,
                ProjectName = a.ProjectName
            })
            .ToList();

        var multiGroups = items
            .Where(a => a.QuestionType == "MultipleChoice")
            .GroupBy(a => new { a.YouthId, a.QuestionText, a.ProjectName })
            .Select(g => new AnswerListItemDto
            {
                AnswerId = g.First().AnswerId,
                QuestionText = g.Key.QuestionText,
                QuestionType = "MultipleChoice",
                Value = string.Join("; ", g.Select(a => a.Value)),
                YouthId = g.Key.YouthId,
                YouthEmail = g.First().YouthEmail,
                ProjectName = g.Key.ProjectName
            })
            .ToList();

        return singleItems.Concat(multiGroups).ToList();
    }

    public List<IdeaStatDto> GetIdeaStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var ideas = _repo.GetIdeaStats(workspaceId, projectId, ToRepoFilters(filters));

        return ideas.Select(i => new IdeaStatDto
        {
            Id = i.Id,
            Content = i.Content,
            Summary = i.Summary,
            Status = i.Status,
            SubmissionDate = i.SubmissionDate,
            TopicName = i.TopicName,
            SemanticCategories = i.SemanticCategories,
            YouthId = i.YouthId,
            YouthEmail = i.YouthEmail,
            MarkedForReview = i.MarkedForReview,
            RejectionReason = i.RejectionReason
        }).ToList();
    }

    public List<IdeaCountDto> GetIdeasByTopic(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var stats = _repo.GetIdeaCountByTopic(workspaceId, projectId, ToRepoFilters(filters));

        return stats.Select(s => new IdeaCountDto { Label = s.TopicName, Count = s.Count }).ToList();
    }

    public List<IdeaCountDto> GetIdeasByStatus(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var stats = _repo.GetIdeaCountByStatus(workspaceId, projectId, ToRepoFilters(filters));

        return stats.Select(s => new IdeaCountDto { Label = s.Status, Count = s.Count }).ToList();
    }

    public List<IdeaCountDto> GetIdeasByCategory(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var stats = _repo.GetIdeaCountByCategory(workspaceId, projectId, ToRepoFilters(filters));

        return stats.Select(s => new IdeaCountDto { Label = s.Category, Count = s.Count }).ToList();
    }

    public ParticipationStatsDto GetParticipationStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null)
    {
        var stats = _repo.GetParticipationStats(workspaceId, projectId, ToRepoFilters(filters));

        return new ParticipationStatsDto
        {
            TotalYouth = stats.TotalYouth,
            YouthWithAnswers = stats.YouthWithAnswers,
            YouthWithIdeas = stats.YouthWithIdeas,
            YouthWithBoth = stats.YouthWithBoth,
            ConversionRate = Math.Round(stats.ConversionRate, 1),
            AvgAnswersPerYouth = Math.Round(stats.AvgAnswersPerYouth, 1),
            AvgIdeasPerYouth = Math.Round(stats.AvgIdeasPerYouth, 1)
        };
    }

    public List<PlatformWorkspaceStatDto> GetPlatformStats(Slug? workspaceId = null)
    {
        var stats = _repo.GetPlatformStats(workspaceId);

        return stats.Select(s => new PlatformWorkspaceStatDto
        {
            WorkspaceSlug = s.WorkspaceSlug,
            WorkspaceName = s.WorkspaceName,
            ProjectCount = s.ProjectCount,
            YouthCount = s.YouthCount,
            IdeaCount = s.IdeaCount,
            AnswerCount = s.AnswerCount,
            ConversionRate = Math.Round(s.ConversionRate, 1)
        }).ToList();
    }

    public async Task<AiSummaryResponseDto> GenerateIdeaSummaryAsync(Slug workspaceId, Slug? projectId, AiSummaryRequestDto request, AnalyticsFilterRequest filters)
    {
        const int maxIdeas = 50;
        var ideas = _repo.GetIdeaContentsForSummary(workspaceId, projectId, maxIdeas, ToRepoFilters(filters));
        var ideasList = ideas.ToList();

        if (ideasList.Count == 0)
        {
            return new AiSummaryResponseDto
            {
                Overview = "No approved ideas available for summary generation.",
                Trends = new List<string>(),
                MinorityViews = new List<string>(),
                NotableQuotes = new List<string>(),
                SuggestedActions = new List<string>()
            };
        }

        var language = request.Language ?? "English";
        var variables = AiPromptDefaults.BuildIdeaSummaryVariables(ideasList, request.Focus, language);

        var systemContent = await LoadPromptOrDefaultAsync(
            AiPromptKeys.AnalyticsIdeaSummarySystem,
            AiPromptDefaults.BuildIdeaSummarySystemPrompt(),
            variables);

        var userContent = await LoadPromptOrDefaultAsync(
            AiPromptKeys.AnalyticsIdeaSummaryUser,
            AiPromptDefaults.BuildIdeaSummaryUserPrompt(ideasList, request.Focus, language),
            variables);

        var workspaceIdStr = workspaceId.Text;
        var projectIdStr = projectId?.Text;
        var rawResponse = await _aiManager.CompletePlainTextAsync(systemContent, userContent, workspaceIdStr, projectIdStr, "AnalyticsIdeaSummary");

        return ParseSummaryJson(rawResponse);
    }

    public async Task<AiSummaryResponseDto> GetCachedSummaryAsync(Slug workspaceId, Slug? projectId)
    {
        var saved = await _repo.GetSavedSummaryAsync(workspaceId, projectId);
        if (saved == null) return null;

        return new AiSummaryResponseDto
        {
            Overview = saved.Overview,
            Trends = DeserializeList(saved.TrendsJson),
            MinorityViews = DeserializeList(saved.MinorityViewsJson),
            NotableQuotes = DeserializeList(saved.NotableQuotesJson),
            SuggestedActions = DeserializeList(saved.SuggestedActionsJson),
            GeneratedAt = saved.GeneratedAt
        };
    }

    public async Task SaveSummaryAsync(Slug workspaceId, Slug? projectId, AiSummaryRequestDto request, AiSummaryResponseDto response)
    {
        var saved = new SavedAiSummary
        {
            WorkspaceId = workspaceId,
            ProjectId = projectId,
            Focus = request.Focus ?? string.Empty,
            Language = request.Language ?? "English",
            Overview = response.Overview.Length > 2000 ? response.Overview[..2000] : response.Overview,
            TrendsJson = SerializeList(response.Trends),
            MinorityViewsJson = SerializeList(response.MinorityViews),
            NotableQuotesJson = SerializeList(response.NotableQuotes),
            SuggestedActionsJson = SerializeList(response.SuggestedActions),
            GeneratedAt = DateTime.UtcNow
        };

        await _repo.SaveSummaryAsync(saved);
    }

    private static string SerializeList(List<string> list)
    {
        return JsonSerializer.Serialize(list);
    }

    private static List<string> DeserializeList(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
        catch { return new List<string>(); }
    }

    public string ExportQuantitativeCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Type,Question,Choice,Count");
        var choiceStats = _repo.GetChoiceQuestionStats(workspaceId, projectId, ToRepoFilters(filters));
        foreach (var stat in choiceStats)
        {
            sb.AppendLine($"\"{stat.QuestionType}\",\"{EscapeCsv(stat.QuestionText)}\",\"{EscapeCsv(stat.ChoiceText ?? "")}\",{stat.Count}");
        }

        sb.AppendLine();
        sb.AppendLine("Type,Question,Average,LowerBound,UpperBound,Count");
        var scaleStats = _repo.GetScaleQuestionStats(workspaceId, projectId, ToRepoFilters(filters));
        foreach (var stat in scaleStats)
        {
            sb.AppendLine($"Scale,\"{EscapeCsv(stat.QuestionText)}\",{stat.Average:F1},{stat.LowerBound},{stat.UpperBound},{stat.Count}");
        }

        return sb.ToString();
    }

    public string ExportAnswersOnlyCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters, Guid? youthId = null, string questionType = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Type,Project,Question,Content,Youth");
        var allAnswers = _repo.GetAllAnswerItems(workspaceId, projectId, ToRepoFilters(filters));
        foreach (var a in allAnswers)
        {
            if (youthId.HasValue && a.YouthId != youthId.Value) continue;
            if (!string.IsNullOrWhiteSpace(questionType) && !string.Equals(a.QuestionType, questionType, StringComparison.OrdinalIgnoreCase)) continue;
            var youth = a.YouthEmail ?? (a.YouthId.HasValue ? a.YouthId.Value.ToString("N")[..8] : "");
            sb.AppendLine($"\"{a.QuestionType}\",\"{EscapeCsv(a.ProjectName)}\",\"{EscapeCsv(a.QuestionText)}\",\"{EscapeCsv(a.Value)}\",\"{youth}\"");
        }
        return sb.ToString();
    }

    public string ExportIdeasOnlyCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters, Guid? youthId = null, string category = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Type,Topic,Status,Content,Summary,Categories,Youth,Date");
        var ideas = _repo.GetIdeaStats(workspaceId, projectId, ToRepoFilters(filters));
        foreach (var i in ideas)
        {
            if (youthId.HasValue && i.YouthId != youthId.Value) continue;
            if (!string.IsNullOrWhiteSpace(category) && !i.SemanticCategories.Contains(category, StringComparer.OrdinalIgnoreCase)) continue;
            var cats = string.Join("; ", i.SemanticCategories);
            var youth = i.YouthEmail ?? (i.YouthId.HasValue ? i.YouthId.Value.ToString("N")[..8] : "");
            sb.AppendLine($"Idea,\"{EscapeCsv(i.TopicName ?? "")}\",\"{i.Status}\",\"{EscapeCsv(i.Content)}\",\"{EscapeCsv(i.Summary)}\",\"{EscapeCsv(cats)}\",\"{youth}\",{i.SubmissionDate:yyyy-MM-dd}");
        }
        return sb.ToString();
    }

    public string ExportQualitativeCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters, Guid? youthId = null, string category = null, string questionType = null)
    {
        var sb = new StringBuilder();
        sb.Append(ExportAnswersOnlyCsv(workspaceId, projectId, filters, youthId, questionType));
        sb.AppendLine();
        sb.Append(ExportIdeasOnlyCsv(workspaceId, projectId, filters, youthId, category));
        return sb.ToString();
    }

    public string ExportCombinedCsv(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters, Guid? youthId = null, string category = null, string questionType = null)
    {
        var sb = new StringBuilder();
        sb.Append(ExportQuantitativeCsv(workspaceId, projectId, filters));
        sb.AppendLine();
        sb.Append(ExportQualitativeCsv(workspaceId, projectId, filters, youthId, category, questionType));
        return sb.ToString();
    }

    private static AnalyticsFilterParams ToRepoFilters(AnalyticsFilterRequest request)
    {
        if (request == null) return null;

        return new AnalyticsFilterParams
        {
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            TopicId = request.TopicId,
            Status = request.Status
        };
    }

    private async Task<string> LoadPromptOrDefaultAsync(string promptName, string defaultValue, IReadOnlyDictionary<string, string> variables)
    {
        try
        {
            var existingPrompts = await _promptRepository.GetAllPromptsAsync();
            var prompt = existingPrompts.FirstOrDefault(p => p.Name == promptName);

            if (prompt != null)
            {
                if (!string.IsNullOrWhiteSpace(prompt.SystemPrompt) && promptName.Contains("System"))
                    return AiPromptDefaults.BuildIdeaSummarySystemPrompt().Contains("{{")
                        ? PromptRenderer.Render(prompt.SystemPrompt, variables)
                        : prompt.SystemPrompt;
                if (!string.IsNullOrWhiteSpace(prompt.UserPromptTemplate) && promptName.Contains("User"))
                    return PromptRenderer.Render(prompt.UserPromptTemplate, variables);
            }
        }
        catch
        {
            return defaultValue;
        }

        return defaultValue;
    }

    private static AiSummaryResponseDto ParseSummaryJson(string json)
    {
        try
        {
            json = json.Trim();
            var startIndex = json.IndexOf('{');
            var endIndex = json.LastIndexOf('}');
            if (startIndex >= 0 && endIndex > startIndex)
                json = json[startIndex..(endIndex + 1)];

            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new AiSummaryResponseDto
            {
                Overview = GetString(root, "overview"),
                Trends = GetStringList(root, "trends"),
                MinorityViews = GetStringList(root, "minorityViews"),
                NotableQuotes = GetStringList(root, "notableQuotes"),
                SuggestedActions = GetStringList(root, "suggestedActions")
            };
        }
        catch
        {
            return new AiSummaryResponseDto
            {
                Overview = json.Length > 300 ? json[..300] + "..." : json,
                Trends = new List<string>(),
                MinorityViews = new List<string>(),
                NotableQuotes = new List<string>(),
                SuggestedActions = new List<string>()
            };
        }
    }

    private static string GetString(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? string.Empty;
        return string.Empty;
    }

    private static List<string> GetStringList(JsonElement element, string property)
    {
        if (element.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.Array)
        {
            return prop.EnumerateArray()
                .Select(e => e.GetString() ?? string.Empty)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
        return new List<string>();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }

    public PlatformModerationStatsDto GetPlatformModerationStats(Slug? workspaceId = null)
    {
        var stats = _repo.GetPlatformModerationStats(workspaceId);
        return new PlatformModerationStatsDto
        {
            TotalFlaggedIdeas = stats.TotalFlaggedIdeas,
            TotalFlaggedComments = stats.TotalFlaggedComments,
            TotalIdeas = stats.TotalIdeas,
            TotalComments = stats.TotalComments,
            IdeaFlags = stats.IdeaFlags.Select(f => new IdeaCountDto { Label = f.Label, Count = f.Count }).ToList(),
            CommentFlags = stats.CommentFlags.Select(f => new IdeaCountDto { Label = f.Label, Count = f.Count }).ToList()
        };
    }

    public PlatformUserStatsDto GetPlatformUserStats(Slug? workspaceId = null)
    {
        var stats = _repo.GetPlatformUserStats(workspaceId);
        return new PlatformUserStatsDto
        {
            TotalYouth = stats.TotalYouth,
            YouthWithIdeas = stats.YouthWithIdeas,
            YouthWithAnswers = stats.YouthWithAnswers,
            YouthWithBoth = stats.YouthWithBoth,
            AvgAnswersPerYouth = stats.AvgAnswersPerYouth,
            AvgIdeasPerYouth = stats.AvgIdeasPerYouth,
            ConversionRate = stats.ConversionRate
        };
    }

    public List<UsageTrendPointDto> GetUsageTrend(Slug? workspaceId = null, Slug? projectId = null, DateTime? from = null, DateTime? to = null)
    {
        var points = _repo.GetUsageTrend(workspaceId, projectId, from, to);
        return points.Select(p => new UsageTrendPointDto
        {
            Date = p.Date.ToString("yyyy-MM-dd"),
            IdeaCount = p.IdeaCount,
            UniqueYouth = p.UniqueYouth
        }).ToList();
    }

    public List<ModerationQueueItemDto> GetModerationQueue(Slug workspaceId, Slug? projectId, int? topicId, int? ideaId)
    {
        var items = _repo.GetModerationQueue(workspaceId, projectId, topicId, ideaId);
        return items.Select(q => new ModerationQueueItemDto
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
        }).ToList();
    }

    public async Task<bool> SetModerationStatusAsync(string type, int id, string status, string reason = null)
    {
        return await _repo.SetModerationStatusAsync(type, id, status, reason);
    }

    public async Task<bool> ToggleMarkedForReviewAsync(string type, int id)
    {
        return await _repo.ToggleMarkedForReviewAsync(type, id);
    }

    public IReadOnlyCollection<Topic> GetTopicsForWorkspace(Slug workspaceId)
    {
        return _repo.GetTopicsForWorkspace(workspaceId);
    }

    public IReadOnlyList<IdeaCountDto> GetToxicityStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null)
    {
        return _repo.GetToxicityStats(workspaceId, projectId, ToRepoFilters(filters))
            .Select(t => new IdeaCountDto { Label = t.Label, Count = t.Count })
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<IdeaCountDto> GetResponseToxicityStats(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null)
    {
        return _repo.GetResponseToxicityStats(workspaceId, projectId, ToRepoFilters(filters))
            .Select(t => new IdeaCountDto { Label = t.Label, Count = t.Count })
            .ToList()
            .AsReadOnly();
    }

    public int GetDistinctFlaggedIdeaCount(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null)
    {
        return _repo.GetDistinctFlaggedIdeaCount(workspaceId, projectId, ToRepoFilters(filters));
    }

    public int GetDistinctFlaggedResponseCount(Slug workspaceId, Slug? projectId, AnalyticsFilterRequest filters = null)
    {
        return _repo.GetDistinctFlaggedResponseCount(workspaceId, projectId, ToRepoFilters(filters));
    }

    public int GetTotalComments(Slug workspaceId, Slug? projectId)
    {
        return _repo.GetTotalComments(workspaceId, projectId);
    }

    public double GetEmailPercentage(Slug workspaceId, Slug? projectId)
    {
        return _repo.GetEmailPercentage(workspaceId, projectId);
    }

    public IReadOnlyCollection<Youth> GetYouthList(Slug workspaceId, Slug? projectId)
    {
        return _repo.GetYouthList(workspaceId, projectId);
    }

    public IReadOnlyCollection<string> GetDistinctCategories(Slug workspaceId, Slug? projectId)
    {
        return _repo.GetDistinctCategories(workspaceId, projectId);
    }

    public IReadOnlyCollection<string> GetDistinctQuestionTypes(Slug workspaceId, Slug? projectId)
    {
        return _repo.GetDistinctQuestionTypes(workspaceId, projectId);
    }

    public IReadOnlyCollection<IdeaResponse> GetResponsesForIdeas(HashSet<int> ideaIds)
    {
        return _repo.GetResponsesForIdeas(ideaIds);
    }

    public HashSet<int> GetIdeaIdsCommentedByYouth(Guid youthId, HashSet<Slug> projectIds)
    {
        return _repo.GetIdeaIdsCommentedByYouth(youthId, projectIds);
    }
}
