using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conversey.DAL.Analytics;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ConverseyDbContext _db;

    public AnalyticsRepository(ConverseyDbContext db)
    {
        _db = db;
    }

    private static DateTime? EnsureUtc(DateTime? dt)
    {
        if (!dt.HasValue) return null;
        if (dt.Value.Kind == DateTimeKind.Utc) return dt;
        return DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
    }

    private HashSet<Slug> GetProjectIds(Slug workspaceId, Slug? projectId)
    {
        if (projectId.HasValue)
            return new HashSet<Slug> { projectId.Value };

        return _db.Projects
            .Where(p => p.Workspace.Id == workspaceId)
            .Select(p => p.Id)
            .ToHashSet();
    }

    public IReadOnlyCollection<QuestionChoiceStat> GetChoiceQuestionStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<QuestionChoiceStat>();

        var result = new List<QuestionChoiceStat>();

        var singleAnswers = _db.Answers
            .OfType<SingleChoiceAnswer>()
            .Include(a => a.Question).ThenInclude(q => q.Project)
            .Include(a => a.Value)
            .Include(a => a.Youth)
            .ToList()
            .Where(a => a.Question != null && a.Question.Project != null && projectIds.Contains(a.Question.Project.Id))
            .ToList();

        foreach (var a in singleAnswers)
        {
            result.Add(new QuestionChoiceStat
            {
                QuestionId = a.Question!.Id,
                QuestionText = a.Question.Text,
                QuestionType = "SingleChoice",
                ChoiceId = a.Value?.Id ?? 0,
                ChoiceText = a.Value?.Text ?? string.Empty,
                Count = 1
            });
        }

        var multiAnswers = _db.Answers
            .OfType<MultipleChoiceAnswer>()
            .Include(a => a.Question).ThenInclude(q => q.Project)
            .Include(a => a.Value)
            .Include(a => a.Youth)
            .ToList()
            .Where(a => a.Question != null && a.Question.Project != null && projectIds.Contains(a.Question.Project.Id))
            .ToList();

        foreach (var a in multiAnswers)
        {
            if (a.Value == null) continue;
            foreach (var choice in a.Value)
            {
                result.Add(new QuestionChoiceStat
                {
                    QuestionId = a.Question!.Id,
                    QuestionText = a.Question.Text,
                    QuestionType = "MultipleChoice",
                    ChoiceId = choice.Id,
                    ChoiceText = choice.Text,
                    Count = 1
                });
            }
        }

        return result
            .GroupBy(r => new { r.QuestionId, r.QuestionText, r.QuestionType, r.ChoiceId, r.ChoiceText })
            .Select(g => new QuestionChoiceStat
            {
                QuestionId = g.Key.QuestionId,
                QuestionText = g.Key.QuestionText,
                QuestionType = g.Key.QuestionType,
                ChoiceId = g.Key.ChoiceId,
                ChoiceText = g.Key.ChoiceText,
                Count = g.Sum(x => x.Count)
            })
            .OrderBy(r => r.QuestionId)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<ScaleStat> GetScaleQuestionStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<ScaleStat>();

        var scaleQuestions = _db.Questions
            .OfType<ScaleQuestion>()
            .Where(q => projectIds.Contains(EF.Property<Slug>(q, "ProjectId")))
            .ToList();

        var allIntAnswers = _db.Answers
            .OfType<Answer<int>>()
            .Include(a => a.Question)
            .ToList();

        var result = new List<ScaleStat>();
        foreach (var sq in scaleQuestions)
        {
            var answersForQ = allIntAnswers
                .Where(a => a.Question != null && a.Question.Id == sq.Id)
                .Select(a => a.Value)
                .ToList();

            var distribution = new Dictionary<int, int>();
            for (var i = sq.LowerBound; i <= sq.UpperBound; i++)
                distribution[i] = 0;
            foreach (var v in answersForQ)
            {
                if (distribution.ContainsKey(v))
                    distribution[v]++;
            }

            result.Add(new ScaleStat
            {
                QuestionId = sq.Id,
                QuestionText = sq.Text,
                LowerBound = sq.LowerBound,
                UpperBound = sq.UpperBound,
                Average = answersForQ.Count > 0 ? answersForQ.Average() : 0,
                Count = answersForQ.Count,
                Distribution = distribution
            });
        }

        return result.AsReadOnly();
    }

    public IReadOnlyCollection<OpenAnswerItem> GetOpenAnswers(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<OpenAnswerItem>();

        var openQuestionIds = _db.Questions
            .OfType<OpenQuestion>()
            .Where(q => projectIds.Contains(EF.Property<Slug>(q, "ProjectId")))
            .Select(q => q.Id)
            .ToHashSet();

        return _db.Answers
            .OfType<Answer<string>>()
            .Include(a => a.Question)
            .Include(a => a.Youth)
            .ToList()
            .Where(a => a.Question != null && openQuestionIds.Contains(a.Question.Id))
            .Select(a => new OpenAnswerItem
            {
                AnswerId = a.Id,
                QuestionText = a.Question!.Text,
                QuestionType = "OpenText",
                Value = a.Value ?? string.Empty,
                YouthId = a.Youth?.Id,
                YouthEmail = a.Youth?.Email
            })
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<IdeaStatItem> GetIdeaStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<IdeaStatItem>();

        var query = _db.Ideas
            .Include(i => i.Topic)
            .Include(i => i.Youth)
            .AsQueryable()
            .Where(i => projectIds.Contains(i.Project.Id));

        if (filters?.TopicId.HasValue == true)
            query = query.Where(i => i.Topic.Id == filters.TopicId.Value);

        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) query = query.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) query = query.Where(i => i.SubmissionDate <= dt.Value);

        if (!string.IsNullOrWhiteSpace(filters?.Status))
        {
            if (Enum.TryParse<ModerationStatus>(filters.Status, true, out var status))
                query = query.Where(i => i.Status == status);
        }

        return query
            .OrderByDescending(i => i.SubmissionDate)
            .Select(i => new IdeaStatItem
            {
                Id = i.Id,
                Content = i.Content,
                Summary = i.Summary ?? string.Empty,
                Status = i.Status.ToString(),
                SubmissionDate = i.SubmissionDate,
                TopicName = i.Topic.Name,
                SemanticCategories = i.SemanticCategories ?? Array.Empty<string>(),
                YouthId = i.Youth.Id,
                YouthEmail = i.Youth.Email,
                MarkedForReview = i.MarkedForReview,
                RejectionReason = i.RejectionReason
            })
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<IdeaCountByTopic> GetIdeaCountByTopic(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<IdeaCountByTopic>();

        var query = _db.Ideas.Include(i => i.Topic)
            .Where(i => projectIds.Contains(i.Project.Id)).AsQueryable();

        if (filters?.TopicId.HasValue == true)
            query = query.Where(i => i.Topic.Id == filters.TopicId.Value);
        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) query = query.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) query = query.Where(i => i.SubmissionDate <= dt.Value);
        if (!string.IsNullOrWhiteSpace(filters?.Status) && Enum.TryParse<ModerationStatus>(filters.Status, true, out var st))
            query = query.Where(i => i.Status == st);

        return query
            .GroupBy(i => i.Topic.Name)
            .Select(g => new IdeaCountByTopic { TopicName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<IdeaCountByStatus> GetIdeaCountByStatus(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<IdeaCountByStatus>();

        var query = _db.Ideas
            .Where(i => projectIds.Contains(i.Project.Id)).AsQueryable();

        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) query = query.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) query = query.Where(i => i.SubmissionDate <= dt.Value);
        if (!string.IsNullOrWhiteSpace(filters?.Status) && Enum.TryParse<ModerationStatus>(filters.Status, true, out var st))
            query = query.Where(i => i.Status == st);

        return query
            .GroupBy(i => i.Status)
            .Select(g => new IdeaCountByStatus { Status = g.Key.ToString(), Count = g.Count() })
            .ToList().AsReadOnly();
    }

    public IReadOnlyCollection<IdeaCountByCategory> GetIdeaCountByCategory(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters)
    {
        var ideas = GetIdeaStats(workspaceId, projectId, filters);

        return ideas
            .Where(i => i.SemanticCategories.Length > 0)
            .SelectMany(i => i.SemanticCategories)
            .GroupBy(c => c)
            .Select(g => new IdeaCountByCategory { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList().AsReadOnly();
    }

    public ParticipationStats GetParticipationStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return new ParticipationStats();

        var youthIds = _db.Youths
            .Where(y => projectIds.Contains(y.Project.Id))
            .Select(y => y.Id).ToList();

        var totalYouth = youthIds.Count;
        var youthIdSet = youthIds.ToHashSet();

        var allAnswers = _db.Answers.Include("Youth").ToList();
        var totalAnswers = 0;
        var youthWithAnswersSet = new HashSet<Guid>();
        foreach (var a in allAnswers)
        {
            if (!youthIdSet.Contains(a.Youth.Id)) continue;
            totalAnswers++;
            youthWithAnswersSet.Add(a.Youth.Id);
        }

        var ideaQuery = _db.Ideas.Include(i => i.Youth)
            .Where(i => youthIdSet.Contains(i.Youth.Id)).AsQueryable();

        if (filters?.TopicId.HasValue == true)
            ideaQuery = ideaQuery.Where(i => i.Topic.Id == filters.TopicId.Value);
        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) ideaQuery = ideaQuery.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) ideaQuery = ideaQuery.Where(i => i.SubmissionDate <= dt.Value);
        if (!string.IsNullOrWhiteSpace(filters?.Status) && Enum.TryParse<ModerationStatus>(filters.Status, true, out var st))
            ideaQuery = ideaQuery.Where(i => i.Status == st);

        var ideas = ideaQuery.ToList();

        var youthWithIdeasSet = new HashSet<Guid>(ideas.Select(i => i.Youth.Id));
        var totalIdeas = ideas.Count;

        var hasIdeaFilter = filters != null && (!string.IsNullOrWhiteSpace(filters.Status) || filters.TopicId.HasValue || df.HasValue || dt.HasValue);
        if (hasIdeaFilter)
            totalYouth = youthWithIdeasSet.Count;

        var youthWithBoth = youthWithAnswersSet.Intersect(youthWithIdeasSet).Count();

        return new ParticipationStats
        {
            TotalYouth = totalYouth,
            YouthWithAnswers = youthWithAnswersSet.Count,
            YouthWithIdeas = youthWithIdeasSet.Count,
            YouthWithBoth = youthWithBoth,
            ConversionRate = youthWithAnswersSet.Count > 0 ? (double)youthWithBoth / youthWithAnswersSet.Count * 100 : 0,
            AvgAnswersPerYouth = totalYouth > 0 ? (double)totalAnswers / totalYouth : 0,
            AvgIdeasPerYouth = totalYouth > 0 ? (double)totalIdeas / totalYouth : 0
        };
    }

    public IReadOnlyCollection<PlatformWorkspaceStat> GetPlatformStats(Slug? workspaceId = null)
    {
        var workspaces = _db.Workspaces.ToList();
        if (workspaceId.HasValue)
            workspaces = workspaces.Where(w => w.Id == workspaceId.Value).ToList();

        var allYouth = _db.Youths.Include(y => y.Project).ToList();
        var allAnswers = _db.Answers.Include("Youth").ToList();
        var allIdeas = _db.Ideas.Include(i => i.Youth).ToList();

        var result = new List<PlatformWorkspaceStat>();
        foreach (var ws in workspaces)
        {
            var wsYouth = allYouth.Where(y => y.Project.Workspace.Id == ws.Id).ToList();
            var wsYouthIdSet = wsYouth.Select(y => y.Id).ToHashSet();
            var wsProjects = _db.Projects.Where(p => p.Workspace.Id == ws.Id).ToList();

            var wsIdeas = allIdeas.Where(i => wsYouthIdSet.Contains(i.Youth.Id)).ToList();

            var answerYouthSet = new HashSet<Guid>();
            var answerCount = 0;
            foreach (var a in allAnswers)
            {
                if (!wsYouthIdSet.Contains(a.Youth.Id)) continue;
                answerCount++;
                answerYouthSet.Add(a.Youth.Id);
            }

            var ideaYouthSet = new HashSet<Guid>(wsIdeas.Select(i => i.Youth.Id));
            var youthWithBoth = answerYouthSet.Intersect(ideaYouthSet).Count();

            result.Add(new PlatformWorkspaceStat
            {
                WorkspaceSlug = ws.Id.Text,
                WorkspaceName = ws.Name,
                ProjectCount = wsProjects.Count,
                YouthCount = wsYouth.Count,
                IdeaCount = wsIdeas.Count,
                AnswerCount = answerCount,
                ConversionRate = answerYouthSet.Count > 0 ? (double)youthWithBoth / answerYouthSet.Count * 100 : 0
            });
        }

        return result.AsReadOnly();
    }

    public IReadOnlyCollection<string> GetIdeaContentsForSummary(Slug workspaceId, Slug? projectId, int maxIdeas, AnalyticsFilterParams? filters)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<string>();

        var query = _db.Ideas
            .Where(i => projectIds.Contains(i.Project.Id))
            .Where(i => i.Status == ModerationStatus.Approved).AsQueryable();

        if (filters?.TopicId.HasValue == true)
            query = query.Where(i => i.Topic.Id == filters.TopicId.Value);
        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) query = query.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) query = query.Where(i => i.SubmissionDate <= dt.Value);

        return query.OrderBy(i => i.SubmissionDate).Take(maxIdeas).Select(i => i.Content).ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Topic> GetTopicsForWorkspace(Slug workspaceId)
    {
        return _db.Topics
            .Include(t => t.Project).ThenInclude(p => p.Workspace)
            .Where(t => t.Project.Workspace.Id == workspaceId)
            .ToList().AsReadOnly();
    }

    public IReadOnlyList<ToxicityCount> GetToxicityStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<ToxicityCount>();

        var query = _db.Ideas
            .Where(i => projectIds.Contains(i.Project.Id))
            .AsQueryable();

        if (filters?.TopicId.HasValue == true)
            query = query.Where(i => i.Topic.Id == filters.TopicId.Value);
        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) query = query.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) query = query.Where(i => i.SubmissionDate <= dt.Value);
        if (!string.IsNullOrWhiteSpace(filters?.Status) && Enum.TryParse<ModerationStatus>(filters.Status, true, out var st))
            query = query.Where(i => i.Status == st);

        var ideas = query.ToList()
            .Where(i => i.ModerationInfo.Sexual || i.ModerationInfo.HateAndDiscrimination || i.ModerationInfo.ViolenceAndThreats || i.ModerationInfo.DangerousAndCriminalContent || i.ModerationInfo.SelfHarm || i.ModerationInfo.Pii)
            .ToList();

        return new List<ToxicityCount>
        {
            new() { Label = "Hate & Discrimination", Count = ideas.Count(i => i.ModerationInfo.HateAndDiscrimination) },
            new() { Label = "Violence & Threats", Count = ideas.Count(i => i.ModerationInfo.ViolenceAndThreats) },
            new() { Label = "Sexual Content", Count = ideas.Count(i => i.ModerationInfo.Sexual) },
            new() { Label = "Self Harm", Count = ideas.Count(i => i.ModerationInfo.SelfHarm) },
            new() { Label = "Dangerous / Criminal", Count = ideas.Count(i => i.ModerationInfo.DangerousAndCriminalContent) },
            new() { Label = "PII", Count = ideas.Count(i => i.ModerationInfo.Pii) }
        }.AsReadOnly();
    }

    public IReadOnlyList<ToxicityCount> GetResponseToxicityStats(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<ToxicityCount>();

        var ideaQuery = _db.Ideas
            .Where(i => projectIds.Contains(i.Project.Id))
            .AsQueryable();

        if (filters?.TopicId.HasValue == true)
            ideaQuery = ideaQuery.Where(i => i.Topic.Id == filters.TopicId.Value);
        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) ideaQuery = ideaQuery.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) ideaQuery = ideaQuery.Where(i => i.SubmissionDate <= dt.Value);
        if (!string.IsNullOrWhiteSpace(filters?.Status) && Enum.TryParse<ModerationStatus>(filters.Status, true, out var st))
            ideaQuery = ideaQuery.Where(i => i.Status == st);

        var ideas = ideaQuery.Select(i => i.Id).ToHashSet();

        var responses = _db.Responses
            .Include(r => r.Idea)
            .ToList()
            .Where(r => r.Idea != null && ideas.Contains(r.Idea.Id))
            .ToList();

        return new List<ToxicityCount>
        {
            new() { Label = "Hate & Discrimination", Count = responses.Count(r => r.ModerationInfo.HateAndDiscrimination) },
            new() { Label = "Violence & Threats", Count = responses.Count(r => r.ModerationInfo.ViolenceAndThreats) },
            new() { Label = "Sexual Content", Count = responses.Count(r => r.ModerationInfo.Sexual) },
            new() { Label = "Self Harm", Count = responses.Count(r => r.ModerationInfo.SelfHarm) },
            new() { Label = "Dangerous / Criminal", Count = responses.Count(r => r.ModerationInfo.DangerousAndCriminalContent) },
            new() { Label = "PII", Count = responses.Count(r => r.ModerationInfo.Pii) }
        }.AsReadOnly();
    }

    public int GetTotalComments(Slug workspaceId, Slug? projectId)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return 0;

        var ideaIds = _db.Ideas
            .Where(i => projectIds.Contains(i.Project.Id))
            .Select(i => i.Id)
            .ToHashSet();

        return _db.Responses
            .Include(r => r.Idea)
            .ToList()
            .Count(r => r.Idea != null && ideaIds.Contains(r.Idea.Id));
    }

    public int GetDistinctFlaggedIdeaCount(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return 0;

        var query = _db.Ideas
            .Where(i => projectIds.Contains(i.Project.Id))
            .AsQueryable();

        if (filters?.TopicId.HasValue == true)
            query = query.Where(i => i.Topic.Id == filters.TopicId.Value);
        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) query = query.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) query = query.Where(i => i.SubmissionDate <= dt.Value);
        if (!string.IsNullOrWhiteSpace(filters?.Status) && Enum.TryParse<ModerationStatus>(filters.Status, true, out var st))
            query = query.Where(i => i.Status == st);

        return query.ToList()
            .Count(i => i.ModerationInfo.Sexual || i.ModerationInfo.HateAndDiscrimination || i.ModerationInfo.ViolenceAndThreats || i.ModerationInfo.DangerousAndCriminalContent || i.ModerationInfo.SelfHarm || i.ModerationInfo.Pii);
    }

    public int GetDistinctFlaggedResponseCount(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters = null)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return 0;

        var ideaQuery = _db.Ideas
            .Where(i => projectIds.Contains(i.Project.Id))
            .AsQueryable();

        if (filters?.TopicId.HasValue == true)
            ideaQuery = ideaQuery.Where(i => i.Topic.Id == filters.TopicId.Value);
        var df = EnsureUtc(filters?.DateFrom);
        var dt = EnsureUtc(filters?.DateTo);
        if (df.HasValue) ideaQuery = ideaQuery.Where(i => i.SubmissionDate >= df.Value);
        if (dt.HasValue) ideaQuery = ideaQuery.Where(i => i.SubmissionDate <= dt.Value);
        if (!string.IsNullOrWhiteSpace(filters?.Status) && Enum.TryParse<ModerationStatus>(filters.Status, true, out var st))
            ideaQuery = ideaQuery.Where(i => i.Status == st);

        var ideas = ideaQuery.Select(i => i.Id).ToHashSet();

        return _db.Responses
            .Include(r => r.Idea)
            .ToList()
            .Count(r => r.Idea != null && ideas.Contains(r.Idea.Id)
                && (r.ModerationInfo.Sexual || r.ModerationInfo.HateAndDiscrimination || r.ModerationInfo.ViolenceAndThreats || r.ModerationInfo.DangerousAndCriminalContent || r.ModerationInfo.SelfHarm || r.ModerationInfo.Pii));
    }

    public double GetEmailPercentage(Slug workspaceId, Slug? projectId)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return 0;

        var youth = _db.Youths
            .Where(y => projectIds.Contains(y.Project.Id))
            .ToList();

        if (youth.Count == 0) return 0;

        var withEmail = youth.Count(y => !string.IsNullOrWhiteSpace(y.Email));
        return Math.Round((double)withEmail / youth.Count * 100, 1);
    }

    public IReadOnlyCollection<Youth> GetYouthList(Slug workspaceId, Slug? projectId)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<Youth>();

        return _db.Youths
            .Where(y => projectIds.Contains(y.Project.Id))
            .OrderBy(y => y.Email)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<string> GetDistinctCategories(Slug workspaceId, Slug? projectId)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<string>();

        return _db.Ideas
            .Where(i => projectIds.Contains(i.Project.Id))
            .ToList()
            .Where(i => i.SemanticCategories != null && i.SemanticCategories.Length > 0)
            .SelectMany(i => i.SemanticCategories!)
            .Distinct()
            .OrderBy(c => c)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<string> GetDistinctQuestionTypes(Slug workspaceId, Slug? projectId)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<string>();

        var types = new List<string>();

        var hasOpen = _db.Questions
            .OfType<OpenQuestion>()
            .Any(q => projectIds.Contains(EF.Property<Slug>(q, "ProjectId")));
        if (hasOpen) types.Add("OpenText");

        var hasScale = _db.Questions
            .OfType<ScaleQuestion>()
            .Any(q => projectIds.Contains(EF.Property<Slug>(q, "ProjectId")));
        if (hasScale) types.Add("Scale");

        var hasSingle = _db.Questions
            .OfType<SingleChoiceQuestion>()
            .Any(q => projectIds.Contains(EF.Property<Slug>(q, "ProjectId")));
        if (hasSingle) types.Add("SingleChoice");

        var hasMulti = _db.Questions
            .OfType<MultipleChoiceQuestion>()
            .Any(q => projectIds.Contains(EF.Property<Slug>(q, "ProjectId")));
        if (hasMulti) types.Add("MultipleChoice");

        return types.AsReadOnly();
    }

    public IReadOnlyCollection<AnswerListItem> GetAllAnswerItems(Slug workspaceId, Slug? projectId, AnalyticsFilterParams? filters)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<AnswerListItem>();

        var result = new List<AnswerListItem>();

        var openAnswers = _db.Answers.OfType<Answer<string>>()
            .Include(a => a.Question).ThenInclude(q => q.Project)
            .Include(a => a.Youth)
            .ToList()
            .Where(a => a.Question != null && projectIds.Contains(a.Question.Project.Id));

        var scaleAnswers = _db.Answers.OfType<Answer<int>>()
            .Include(a => a.Question).ThenInclude(q => q.Project)
            .Include(a => a.Youth)
            .ToList()
            .Where(a => a.Question != null && projectIds.Contains(a.Question.Project.Id));

        var singleAnswers = _db.Answers.OfType<SingleChoiceAnswer>()
            .Include(a => a.Question).ThenInclude(q => q.Project)
            .Include(a => a.Value)
            .Include(a => a.Youth)
            .ToList()
            .Where(a => a.Question != null && projectIds.Contains(a.Question.Project.Id));

        var multiAnswers = _db.Answers.OfType<MultipleChoiceAnswer>()
            .Include(a => a.Question).ThenInclude(q => q.Project)
            .Include(a => a.Value)
            .Include(a => a.Youth)
            .ToList()
            .Where(a => a.Question != null && projectIds.Contains(a.Question.Project.Id));

        foreach (var a in openAnswers)
            result.Add(new AnswerListItem
            {
                AnswerId = a.Id, QuestionText = a.Question!.Text, QuestionType = "OpenText",
                Value = a.Value ?? "", YouthId = a.Youth?.Id, YouthEmail = a.Youth?.Email,
                ProjectName = a.Question.Project.Name
            });

        foreach (var a in scaleAnswers)
            result.Add(new AnswerListItem
            {
                AnswerId = a.Id, QuestionText = a.Question!.Text, QuestionType = "Scale",
                Value = a.Value.ToString(), YouthId = a.Youth?.Id, YouthEmail = a.Youth?.Email,
                ProjectName = a.Question.Project.Name
            });

        foreach (var a in singleAnswers)
            result.Add(new AnswerListItem
            {
                AnswerId = a.Id, QuestionText = a.Question!.Text, QuestionType = "SingleChoice",
                Value = a.Value?.Text ?? "", YouthId = a.Youth?.Id, YouthEmail = a.Youth?.Email,
                ProjectName = a.Question.Project.Name
            });

        foreach (var a in multiAnswers)
        {
            if (a.Value == null)
            {
                result.Add(new AnswerListItem
                {
                    AnswerId = a.Id, QuestionText = a.Question!.Text, QuestionType = "MultipleChoice",
                    Value = "", YouthId = a.Youth?.Id, YouthEmail = a.Youth?.Email,
                    ProjectName = a.Question.Project.Name
                });
                continue;
            }

            foreach (var choice in a.Value)
            {
                result.Add(new AnswerListItem
                {
                    AnswerId = a.Id, QuestionText = a.Question!.Text, QuestionType = "MultipleChoice",
                    Value = choice.Text ?? "", YouthId = a.Youth?.Id, YouthEmail = a.Youth?.Email,
                    ProjectName = a.Question.Project.Name
                });
            }
        }

        return result.AsReadOnly();
    }

    public IReadOnlyCollection<IdeaResponse> GetResponsesForIdeas(HashSet<int> ideaIds)
    {
        if (ideaIds.Count == 0) return Array.Empty<IdeaResponse>();

        return _db.Responses
            .Include(r => r.Idea)
            .Include(r => r.Youth)
            .ToList()
            .Where(r => r.Idea != null && ideaIds.Contains(r.Idea.Id))
            .OrderBy(r => r.CreatedAt)
            .ToList()
            .AsReadOnly();
    }

    public HashSet<int> GetIdeaIdsCommentedByYouth(Guid youthId, HashSet<Slug> projectIds)
    {
        var youth = _db.Youths.FirstOrDefault(y => y.Id == youthId);
        if (youth == null) return new HashSet<int>();

        return _db.Responses
            .Include(r => r.Idea)
            .ToList()
            .Where(r => r.Idea != null && r.Youth != null && r.Youth.Id == youthId && projectIds.Contains(r.Idea.Project.Id))
            .Select(r => r.Idea!.Id)
            .Distinct()
            .ToHashSet();
    }

    public async Task<bool> ToggleMarkedForReviewAsync(string type, int id)
    {
        if (type == "idea")
        {
            var idea = await _db.Ideas.FirstOrDefaultAsync(i => i.Id == id);
            if (idea == null) return false;
            idea.Status = idea.Status == ModerationStatus.Pending ? ModerationStatus.Approved : ModerationStatus.Pending;
        }
        else if (type == "response")
        {
            var response = await _db.Responses.FirstOrDefaultAsync(r => r.Id == id);
            if (response == null) return false;
            response.Status = response.Status == ModerationStatus.Pending ? ModerationStatus.Approved : ModerationStatus.Pending;
        }
        else
        {
            return false;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetModerationStatusAsync(string type, int id, string status, string? reason = null)
    {
        if (!Enum.TryParse<ModerationStatus>(status, true, out var newStatus))
            return false;

        if (newStatus == ModerationStatus.Pending)
            return false;

        if (type == "idea")
        {
            var idea = await _db.Ideas.FirstOrDefaultAsync(i => i.Id == id);
            if (idea == null) return false;
            idea.Status = newStatus;
            if (newStatus == ModerationStatus.Rejected)
                idea.RejectionReason = reason;
        }
        else if (type == "response")
        {
            var response = await _db.Responses.FirstOrDefaultAsync(r => r.Id == id);
            if (response == null) return false;
            response.Status = newStatus;
            if (newStatus == ModerationStatus.Rejected)
                response.RejectionReason = reason;
        }
        else
        {
            return false;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public IReadOnlyCollection<ModerationQueueItem> GetModerationQueue(Slug workspaceId, Slug? projectId, int? topicId, int? ideaId)
    {
        var projectIds = GetProjectIds(workspaceId, projectId);
        if (projectIds.Count == 0) return Array.Empty<ModerationQueueItem>();

        var result = new List<ModerationQueueItem>();

        var ideaQuery = _db.Ideas
            .Include(i => i.Topic)
            .Include(i => i.Project)
            .Include(i => i.Youth)
            .Where(i => projectIds.Contains(i.Project.Id))
            .Where(i => i.Status == ModerationStatus.Pending)
            .AsQueryable();

        if (topicId.HasValue)
            ideaQuery = ideaQuery.Where(i => i.Topic.Id == topicId.Value);

        if (ideaId.HasValue)
            ideaQuery = ideaQuery.Where(i => i.Id == ideaId.Value);

        var pendingIdeas = ideaQuery.OrderByDescending(i => i.SubmissionDate).ToList();

        foreach (var idea in pendingIdeas)
        {
            result.Add(new ModerationQueueItem
            {
                Type = "idea",
                Id = idea.Id,
                Content = idea.Content,
                SubmissionDate = idea.SubmissionDate,
                TopicName = idea.Topic?.Name,
                ProjectName = idea.Project?.Name,
                ProjectSlug = idea.Project?.Id.ToString(),
                TopicId = idea.Topic?.Id,
                YouthId = idea.Youth?.Id,
                YouthEmail = idea.Youth?.Email,
                FlagSexual = idea.ModerationInfo.Sexual,
                FlagHate = idea.ModerationInfo.HateAndDiscrimination,
                FlagViolence = idea.ModerationInfo.ViolenceAndThreats,
                FlagDangerous = idea.ModerationInfo.DangerousAndCriminalContent,
                FlagSelfHarm = idea.ModerationInfo.SelfHarm,
                FlagPii = idea.ModerationInfo.Pii,
                RejectionReason = idea.RejectionReason
            });
        }

        var pendingResponsesQuery = _db.Responses
            .Include(r => r.Idea).ThenInclude(i => i.Topic)
            .Include(r => r.Idea).ThenInclude(i => i.Project)
            .Include(r => r.Youth)
            .Where(r => r.Idea != null && projectIds.Contains(r.Idea.Project.Id))
            .Where(r => r.Status == ModerationStatus.Pending)
            .AsQueryable();

        if (topicId.HasValue)
            pendingResponsesQuery = pendingResponsesQuery.Where(r => r.Idea!.Topic.Id == topicId.Value);

        if (ideaId.HasValue)
            pendingResponsesQuery = pendingResponsesQuery.Where(r => r.Idea!.Id == ideaId.Value);

        var pendingResponses = pendingResponsesQuery.OrderByDescending(r => r.CreatedAt).ToList();

        foreach (var response in pendingResponses)
        {
            result.Add(new ModerationQueueItem
            {
                Type = "response",
                Id = response.Id,
                Content = response.Text,
                SubmissionDate = response.CreatedAt,
                TopicName = response.Idea?.Topic?.Name,
                ProjectName = response.Idea?.Project?.Name,
                ProjectSlug = response.Idea?.Project?.Id.ToString(),
                TopicId = response.Idea?.Topic?.Id,
                ParentIdeaId = response.Idea?.Id,
                ParentIdeaContent = response.Idea?.Content,
                YouthId = response.Youth?.Id,
                YouthEmail = response.Youth?.Email,
                FlagSexual = response.ModerationInfo.Sexual,
                FlagHate = response.ModerationInfo.HateAndDiscrimination,
                FlagViolence = response.ModerationInfo.ViolenceAndThreats,
                FlagDangerous = response.ModerationInfo.DangerousAndCriminalContent,
                FlagSelfHarm = response.ModerationInfo.SelfHarm,
                FlagPii = response.ModerationInfo.Pii,
                RejectionReason = response.RejectionReason
            });
        }

        return result.AsReadOnly();
    }

    public async Task<SavedAiSummary?> GetSavedSummaryAsync(Slug workspaceId, Slug? projectId)
    {
        return await _db.SavedAiSummaries
            .Where(s => s.WorkspaceId == workspaceId && s.ProjectId == projectId)
            .OrderByDescending(s => s.GeneratedAt)
            .FirstOrDefaultAsync();
    }

    public async Task SaveSummaryAsync(SavedAiSummary summary)
    {
        var existing = await _db.SavedAiSummaries
            .Where(s => s.WorkspaceId == summary.WorkspaceId && s.ProjectId == summary.ProjectId)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Focus = summary.Focus;
            existing.Language = summary.Language;
            existing.Overview = summary.Overview;
            existing.TrendsJson = summary.TrendsJson;
            existing.MinorityViewsJson = summary.MinorityViewsJson;
            existing.NotableQuotesJson = summary.NotableQuotesJson;
            existing.SuggestedActionsJson = summary.SuggestedActionsJson;
            existing.GeneratedAt = summary.GeneratedAt;
        }
        else
        {
            _db.SavedAiSummaries.Add(summary);
        }

        await _db.SaveChangesAsync();
    }

    public PlatformModerationStats GetPlatformModerationStats(Slug? workspaceId = null)
    {
        var ideasQuery = _db.Ideas.AsQueryable();
        var responsesQuery = _db.Responses.Include(r => r.Idea).AsQueryable();

        if (workspaceId.HasValue)
        {
            ideasQuery = ideasQuery
                .Include(i => i.Project)
                .Where(i => i.Project != null && i.Project.Workspace.Id == workspaceId.Value);

            responsesQuery = responsesQuery
                .Include(r => r.Idea).ThenInclude(i => i.Project)
                .Where(r => r.Idea != null && r.Idea.Project != null && r.Idea.Project.Workspace.Id == workspaceId.Value);
        }

        var ideas = ideasQuery.ToList();
        var allResponses = responsesQuery.ToList();

        var flaggedIdeas = ideas.Where(i => i.ModerationInfo.Sexual || i.ModerationInfo.HateAndDiscrimination || i.ModerationInfo.ViolenceAndThreats || i.ModerationInfo.DangerousAndCriminalContent || i.ModerationInfo.SelfHarm || i.ModerationInfo.Pii).ToList();
        var flaggedResponses = allResponses.Where(r => r.ModerationInfo.Sexual || r.ModerationInfo.HateAndDiscrimination || r.ModerationInfo.ViolenceAndThreats || r.ModerationInfo.DangerousAndCriminalContent || r.ModerationInfo.SelfHarm || r.ModerationInfo.Pii).ToList();

        return new PlatformModerationStats
        {
            TotalFlaggedIdeas = flaggedIdeas.Count,
            TotalFlaggedComments = flaggedResponses.Count,
            TotalIdeas = ideas.Count,
            TotalComments = allResponses.Count,
            IdeaFlags = new List<ToxicityCount>
            {
                new() { Label = "Hate & Discrimination", Count = flaggedIdeas.Count(i => i.ModerationInfo.HateAndDiscrimination) },
                new() { Label = "Violence & Threats", Count = flaggedIdeas.Count(i => i.ModerationInfo.ViolenceAndThreats) },
                new() { Label = "Sexual Content", Count = flaggedIdeas.Count(i => i.ModerationInfo.Sexual) },
                new() { Label = "Self Harm", Count = flaggedIdeas.Count(i => i.ModerationInfo.SelfHarm) },
                new() { Label = "Dangerous / Criminal", Count = flaggedIdeas.Count(i => i.ModerationInfo.DangerousAndCriminalContent) },
                new() { Label = "PII", Count = flaggedIdeas.Count(i => i.ModerationInfo.Pii) }
            },
            CommentFlags = new List<ToxicityCount>
            {
                new() { Label = "Hate & Discrimination", Count = flaggedResponses.Count(r => r.ModerationInfo.HateAndDiscrimination) },
                new() { Label = "Violence & Threats", Count = flaggedResponses.Count(r => r.ModerationInfo.ViolenceAndThreats) },
                new() { Label = "Sexual Content", Count = flaggedResponses.Count(r => r.ModerationInfo.Sexual) },
                new() { Label = "Self Harm", Count = flaggedResponses.Count(r => r.ModerationInfo.SelfHarm) },
                new() { Label = "Dangerous / Criminal", Count = flaggedResponses.Count(r => r.ModerationInfo.DangerousAndCriminalContent) },
                new() { Label = "PII", Count = flaggedResponses.Count(r => r.ModerationInfo.Pii) }
            }
        };
    }

    public PlatformUserStats GetPlatformUserStats(Slug? workspaceId = null)
    {
        var youthQuery = _db.Youths.AsQueryable();
        var answersQuery = _db.Answers.Include("Youth").AsQueryable();
        var ideasQuery = _db.Ideas.Include(i => i.Youth).AsQueryable();

        if (workspaceId.HasValue)
        {
            youthQuery = youthQuery
                .Include(y => y.Project)
                .Where(y => y.Project != null && y.Project.Workspace.Id == workspaceId.Value);
        }

        var allYouth = youthQuery.ToList();
        var youthIdSet = new HashSet<Guid>(allYouth.Select(y => y.Id));
        var allAnswers = answersQuery.Where(a => youthIdSet.Contains(a.Youth.Id)).ToList();
        var allIdeas = ideasQuery.Where(i => youthIdSet.Contains(i.Youth.Id)).ToList();

        var totalYouth = allYouth.Count;
        var youthWithAnswersSet = new HashSet<Guid>(allAnswers.Select(a => a.Youth.Id));
        var youthWithIdeasSet = new HashSet<Guid>(allIdeas.Select(i => i.Youth.Id));
        var youthWithBoth = youthWithAnswersSet.Intersect(youthWithIdeasSet).Count();

        return new PlatformUserStats
        {
            TotalYouth = totalYouth,
            YouthWithAnswers = youthWithAnswersSet.Count,
            YouthWithIdeas = youthWithIdeasSet.Count,
            YouthWithBoth = youthWithBoth,
            AvgAnswersPerYouth = totalYouth > 0 ? Math.Round((double)allAnswers.Count / totalYouth, 1) : 0,
            AvgIdeasPerYouth = totalYouth > 0 ? Math.Round((double)allIdeas.Count / totalYouth, 1) : 0,
            ConversionRate = youthWithAnswersSet.Count > 0 ? Math.Round((double)youthWithBoth / youthWithAnswersSet.Count * 100, 1) : 0
        };
    }

    public IReadOnlyCollection<UsageTrendPoint> GetUsageTrend(Slug? workspaceId = null, Slug? projectId = null, DateTime? from = null, DateTime? to = null)
    {
        var ideasQuery = _db.Ideas.AsQueryable();
        if (projectId.HasValue)
        {
            ideasQuery = ideasQuery.Where(i => i.Project.Id == projectId.Value);
        }
        else if (workspaceId.HasValue)
        {
            var projectIds = _db.Projects.Where(p => p.Workspace.Id == workspaceId.Value).Select(p => p.Id).ToHashSet();
            ideasQuery = ideasQuery.Where(i => projectIds.Contains(i.Project.Id));
        }

        var fromDate = EnsureUtc(from) ?? DateTime.UtcNow.AddMonths(-3);
        var toDate = EnsureUtc(to) ?? DateTime.UtcNow;
        if (to.HasValue) toDate = toDate.Date.AddDays(1);

        var ideas = ideasQuery
            .Include(i => i.Youth)
            .Where(i => i.SubmissionDate >= fromDate && i.SubmissionDate <= toDate)
            .ToList();

        var dailyGroups = ideas
            .GroupBy(i => i.SubmissionDate.Date)
            .OrderBy(g => g.Key);

        var result = new List<UsageTrendPoint>();
        foreach (var group in dailyGroups)
        {
            var dayIdeas = group.ToList();
            var uniqueYouth = new HashSet<Guid>(dayIdeas.Select(i => i.Youth.Id)).Count;

            result.Add(new UsageTrendPoint
            {
                Date = group.Key,
                IdeaCount = dayIdeas.Count,
                UniqueYouth = uniqueYouth
            });
        }

        return result.AsReadOnly();
    }
}


#region SavedAiSummaryConfig
public class SavedAiSummaryConfig : IEntityTypeConfiguration<SavedAiSummary>
{
    public void Configure(EntityTypeBuilder<SavedAiSummary> builder)
    {
        #region Relations

        builder
            .HasOne(s => s.Workspace)
            .WithMany()
            .HasForeignKey(s => s.WorkspaceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(s => s.Project)
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        #endregion
    }
}
#endregion
