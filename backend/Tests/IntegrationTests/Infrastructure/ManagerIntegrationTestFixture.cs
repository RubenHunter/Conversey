using Conversey.BL.Ai;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Ideation;
using Conversey.BL.Survey;
using Conversey.DAL;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Conversey.DAL.Survey;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Conversey.BL.Domain.Ai;

namespace Tests.IntegrationTests.Infrastructure;

public static class ManagerSeedData
{
    public const string WorkspaceName = "Hogeschool Nova";
    public const string ProjectName = "Actieplan Mentaal Welzijn 2026-2027";
    public static readonly Guid YouthToken = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static Slug WorkspaceSlug => Slug.FromName(WorkspaceName);
    public static Slug ProjectSlug => Slug.FromName(ProjectName);
}

public sealed class ManagerIntegrationTestFixture : IDisposable
{
    private readonly TestAiManagerConfig _aiConfig = new();
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;

    public ManagerIntegrationTestFixture()
    {
        var services = new ServiceCollection();

        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        services.AddDbContext<ConverseyDbContext>(options => options.UseSqlite(_connection));

        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IIdeaRepository, IdeaRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();

        services.AddScoped<IWorkspaceManager, WorkspaceManager>();
        services.AddScoped<IProjectManager, ProjectManager>();
        services.AddScoped<IIdeaManager, IdeaManager>();
        services.AddScoped<IQuestionManager, QuestionManager>();

        services.AddSingleton(new AiManagerConfig());
        services.AddSingleton(_aiConfig);
        services.AddScoped<IAiManager>(_ => new TestAiManager(_aiConfig));

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        Seed(dbContext);
    }

    public IServiceScope CreateScope()
    {
        return _serviceProvider.CreateScope();
    }

    public void SetAiModerationBehavior(bool isAllowed, string alternative = "Please rephrase your idea in a respectful way.")
    {
        _aiConfig.IsAllowed = isAllowed;
        _aiConfig.Alternative = alternative;
    }

    public void SetAiCategorizationBehavior(bool throwOnCategorize)
    {
        _aiConfig.ThrowOnCategorize = throwOnCategorize;
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _connection.Dispose();
    }

    private static void Seed(ConverseyDbContext dbContext)
    {
        var workspace = new Workspace
        {
            Id = ManagerSeedData.WorkspaceSlug,
            Name = ManagerSeedData.WorkspaceName,
            Projects = new List<Project>()
        };

        var project = new Project
        {
            Id = ManagerSeedData.ProjectSlug,
            Name = ManagerSeedData.ProjectName,
            Description = "Seed project",
            Status = Status.Active,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(14),
            InteractionForm = InteractionType.Chat,
            Workspace = workspace,
            Topic = new List<Topic>(),
            Questions = new List<Question>(),
            Youth = new List<Youth>()
        };

        var topic = new Topic
        {
            Name = "Studiedruk en evaluatie",
            Context = "Seed topic",
            Project = project,
            Ideas = new List<Idea>()
        };

        var youth = new Youth
        {
            Id = ManagerSeedData.YouthToken,
            Email = "seed@student.nova.be",
            Project = project,
            Ideas = new List<Idea>(),
            Reactions = new List<Reaction>(),
            Responses = new List<IdeaResponse>(),
            Answers = new List<Answer>()
        };

        var question = new OpenQuestion
        {
            Text = "Hoe verbeteren we welzijn?",
            Required = true,
            Project = project
        };

        var idea = new Idea
        {
            Content = "Meer rustplekken op campus.",
            Summary = "",
            SubmissionDate = DateTime.UtcNow,
            Status = ModerationStatus.Approved,
            ModerationInfo = new ModerationInfo(),
            SemanticCategories = new[] { "Wellbeing spaces" },
            Project = project,
            Topic = topic,
            Youth = youth,
            Reactions = new List<IdeaReaction>(),
            Responses = new List<IdeaResponse>()
        };

        var pressureIdea = new Idea
        {
            Content = "Beperk deadlines tijdens de examenperiode.",
            Summary = "",
            SubmissionDate = DateTime.UtcNow.AddMinutes(-5),
            Status = ModerationStatus.Approved,
            ModerationInfo = new ModerationInfo(),
            SemanticCategories = new[] { "Study pressure" },
            Project = project,
            Topic = topic,
            Youth = youth,
            Reactions = new List<IdeaReaction>(),
            Responses = new List<IdeaResponse>()
        };

        var response = new IdeaResponse
        {
            Text = "Goed idee!",
            CreatedAt = DateTime.UtcNow,
            Status = ModerationStatus.Approved,
            ModerationInfo = new ModerationInfo(),
            Idea = idea,
            Youth = youth,
            Reactions = new List<ResponseReaction>()
        };

        dbContext.Workspaces.Add(workspace);
        dbContext.Projects.Add(project);
        dbContext.Topics.Add(topic);
        dbContext.Youths.Add(youth);
        dbContext.Questions.Add(question);
        dbContext.Ideas.Add(idea);
        dbContext.Ideas.Add(pressureIdea);
        dbContext.Responses.Add(response);
        dbContext.SaveChanges();
    }

    private sealed class TestAiManager( TestAiManagerConfig config) : IAiManager
    {
        public Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null)
        {
            return Task.FromResult(config.Alternative);
        }

        public Task<ModerationDecision> ModerateContent(string content)
        {
            return Task.FromResult(new ModerationDecision { IsAllowed = config.IsAllowed, Suggestion = config.Alternative });
        }

        public Task<IdeaNudgeDecision> AssessIdeaNudge(IdeaNudgeAssessmentRequest request)
        {
            return Task.FromResult(new IdeaNudgeDecision { IsApproved = true });
        }

        public Task<IEnumerable<int>> RankIdeasByRelation(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
        {
            if (candidateIdeas.Count == 0 || limit <= 0)
            {
                return Task.FromResult<IEnumerable<int>>(Array.Empty<int>());
            }

            var ordered = Enumerable.Range(0, candidateIdeas.Count);
            if (preferDifferent)
            {
                ordered = ordered.Reverse();
            }

            return Task.FromResult(ordered.Take(limit));
        }

        public Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeas(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
        {
            if (config.ThrowOnCategorize)
            {
                throw new InvalidOperationException("Test categorization failure");
            }

            var result = new Dictionary<int, IReadOnlyList<string>>();
            var canonicalExisting = existingCategories
                .Select(category => (category ?? string.Empty).Trim())
                .Where(category => category.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (int index = 0; index < ideas.Count; index++)
            {
                var text = (ideas[index] ?? string.Empty).ToLowerInvariant();
                var categories = new List<string>();

                if (text.Contains("stress") || text.Contains("druk") || text.Contains("exam") || text.Contains("deadline"))
                    categories.Add("Study Pressure");
                if (text.Contains("coach") || text.Contains("support") || text.Contains("psych") || text.Contains("help"))
                    categories.Add("Support services");
                if (text.Contains("campus") || text.Contains("group") || text.Contains("peer") || text.Contains("connected"))
                    categories.Add("Community & belonging");
                if (text.Contains("online") || text.Contains("digital") || text.Contains("hybrid"))
                    categories.Add("Digital learning");

                if (categories.Count == 0)
                {
                    if (canonicalExisting.Count > 0)
                    {
                        categories.AddRange(canonicalExisting.Take(Math.Max(1, maxCategoriesPerIdea)));
                    }
                    else
                    {
                        categories.Add("General ideas");
                    }
                }

                var normalized = categories
                    .Select(category => canonicalExisting.FirstOrDefault(existing => NormalizeCategoryKey(existing) == NormalizeCategoryKey(category)) ?? category)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(Math.Max(1, maxCategoriesPerIdea))
                    .ToList();

                result[index] = normalized.AsReadOnly();
            }

            return Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<string>>>(result);
        }

        public void Dispose()
        {
        }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "test")));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public object GetService(Type serviceType, object serviceKey = null)
        {
            return null;
        }

        private static string NormalizeCategoryKey(string value)
        {
            return new string((value ?? string.Empty)
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
        }

        public Task<IReadOnlyList<string>> ExtractKeyPhrases(
            string transcript,
            string language,
            int maxPhrases,
            IReadOnlyList<string>? existingPhrases = null,
            IReadOnlyList<string>? rejectedPhrases = null)
        {
            if (string.IsNullOrWhiteSpace(transcript) || maxPhrases <= 0)
                return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

            var rejected = rejectedPhrases?.Select(p => p.Trim().ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();
            var existing = existingPhrases?.Select(p => p.Trim().ToLowerInvariant()).ToHashSet() ?? new HashSet<string>();

            var sentences = transcript
                .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0 && !rejected.Contains(s.ToLowerInvariant()) && !existing.Contains(s.ToLowerInvariant()))
                .Take(maxPhrases)
                .ToList()
                .AsReadOnly();
            return Task.FromResult<IReadOnlyList<string>>(sentences);
        }
    }

    private sealed class TestAiManagerConfig
    {
        public bool IsAllowed { get; set; } = true;
        public string Alternative { get; set; } = "Please rephrase your idea in a respectful way.";
        public bool ThrowOnCategorize { get; set; }
    }
}

