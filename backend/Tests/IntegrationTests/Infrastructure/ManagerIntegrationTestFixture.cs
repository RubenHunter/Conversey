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
using Microsoft.Extensions.DependencyInjection;
using Conversey.BL.Domain.Ai;
using System.Runtime.CompilerServices;

using Conversey.BL.Ai.DTOs;

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
        
        var cloudStorageMock = new Moq.Mock<ICloudStorageRepository>();
        services.AddScoped<ICloudStorageRepository>(_ => cloudStorageMock.Object);

        services.AddScoped<IWorkspaceManager, WorkspaceManager>();
        services.AddScoped<IProjectManager, ProjectManager>();
        services.AddScoped<IIdeaManager, IdeaManager>();
        services.AddScoped<IQuestionManager, QuestionManager>();

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
        public Task<string> GenerateAlternativeAsync(string content, ModerationDecision decision = null, string? workspaceId = null, string? projectId = null)
        {
            return Task.FromResult(config.Alternative);
        }

        public Task<ModerationDecision> ModerateContentAsync(string content, string? workspaceId = null, string? projectId = null)
        {
            return Task.FromResult(new ModerationDecision { IsAllowed = config.IsAllowed, Suggestion = config.Alternative });
        }

        public Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(IdeaNudgeAssessmentRequest request, string? workspaceId = null, string? projectId = null)
        {
            return Task.FromResult(new IdeaNudgeDecision { IsApproved = true });
        }

        public Task<IEnumerable<int>> RankIdeasByRelationAsync(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit, string? workspaceId = null, string? projectId = null)
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

            return Task.FromResult<IEnumerable<int>>(ordered.Take(limit).ToList());
        }

        public Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeasAsync(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea, string? workspaceId = null, string? projectId = null)
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

        private static string NormalizeCategoryKey(string value)
        {
            return new string((value ?? string.Empty)
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray());
        }

        public Task<ExtractKeyPhrasesResponse> ExtractKeyPhrases(
            string transcript,
            Language language,
            int maxPhrases,
            IReadOnlyList<string>? existingPhrases = null,
            IReadOnlyList<string>? rejectedPhrases = null)
        {
            if (string.IsNullOrWhiteSpace(transcript) || maxPhrases <= 0)
                return Task.FromResult(new ExtractKeyPhrasesResponse([]));

            var rejected = rejectedPhrases?.Select(p => p.Trim().ToLowerInvariant()).ToHashSet() ?? [];
            var existing = existingPhrases?.Select(p => p.Trim().ToLowerInvariant()).ToHashSet() ?? [];

            var sentences = transcript
                .Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0 && !rejected.Contains(s.ToLowerInvariant()) && !existing.Contains(s.ToLowerInvariant()))
                .Take(maxPhrases)
                .ToList()
                .AsReadOnly();
            return Task.FromResult(new ExtractKeyPhrasesResponse(sentences));
        }

        public Task<string> GenerateTextFromBubbles(
            string transcript,
            IReadOnlyList<string> bubbles,
            Language language,
            IReadOnlyList<string>? rejectedPhrases = null)
        {
            if (string.IsNullOrWhiteSpace(transcript) || bubbles == null || bubbles.Count == 0)
                return Task.FromResult(string.Empty);
            // Filter out rejected phrases
            var filteredBubbles = bubbles.ToList();
            if (rejectedPhrases != null)
            {
                var rejectedSet = new HashSet<string>(rejectedPhrases, StringComparer.OrdinalIgnoreCase);
                filteredBubbles = filteredBubbles.Where(b => !rejectedSet.Contains(b)).ToList();
            }
            return Task.FromResult(transcript + " " + string.Join(", ", filteredBubbles));
        }

        public Task<string> CompletePlainTextAsync(
            string systemPrompt,
            string userPrompt,
            string? workspaceId = null,
            string? projectId = null,
            string? displayPromptName = null)
        {
            return Task.FromResult("[Test] Plain text completion response.");
        }
    }

    private sealed class TestAiManagerConfig
    {
        public bool IsAllowed { get; set; } = true;
        public string Alternative { get; set; } = "Please rephrase your idea in a respectful way.";
        public bool ThrowOnCategorize { get; set; }
    }
}

