using Conversey.BL.Ai;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.BL.Domain.Survey;
using Conversey.BL.Subplatform;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.BL.Subplatform.Survey.Questions;
using Conversey.DAL;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Conversey.DAL.Survey;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

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
            Slug = ManagerSeedData.ProjectSlug,
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
            Token = ManagerSeedData.YouthToken,
            Email = "seed@student.nova.be",
            Project = project,
            Ideas = new List<Idea>(),
            Reactions = new List<Reaction>(),
            Responses = new List<Conversey.BL.Domain.Ideation.Response>(),
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
            Project = project,
            Topic = topic,
            Youth = youth,
            Reactions = new List<IdeaReaction>(),
            Responses = new List<Conversey.BL.Domain.Ideation.Response>()
        };

        var response = new Conversey.BL.Domain.Ideation.Response
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

        public void Dispose()
        {
        }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "test")));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public object GetService(Type serviceType, object serviceKey = null)
        {
            return null;
        }
    }

    private sealed class TestAiManagerConfig
    {
        public bool IsAllowed { get; set; } = true;
        public string Alternative { get; set; } = "Please rephrase your idea in a respectful way.";
    }
}

