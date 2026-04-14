using Conversey.BL.Administration;
using Conversey.BL.Ai;
using Conversey.BL.Ai.Managers;
using Conversey.BL.Domain.Common;
using Conversey.BL.Ideation;
using Conversey.BL.Subplatform;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Survey;
using Conversey.DAL;
using Conversey.DAL.Subplatform;
using Conversey.DAL.Subplatform.Ai;
using Conversey.DAL.Subplatform.Survey;
using Conversey.DAL.Subplatform.Survey.Ideas;
using Conversey.DAL.Subplatform.Survey.Questions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.IntegrationTests.Infrastructure;

public static class ManagerSeedData
{
    public const string WorkspaceName = "Hogeschool Nova";
    public const string ProjectTitle = "Actieplan Mentaal Welzijn 2026-2027";
    public const string YouthToken = "st-amelie-01";

    public static Slug WorkspaceSlug => Slug.FromName(WorkspaceName);
    public static Slug ProjectSlug => Slug.FromName(ProjectTitle);
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

        services.AddDbContext<ConverseyDbContext>(options =>
            options.UseSqlite(_connection));

        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IIdeaRepository, IdeaRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();

        services.AddScoped<IWorkspaceManager, WorkspaceManager>();
        services.AddScoped<IProjectManager, ProjectManager>();
        services.AddScoped<IIdeaManager, IdeaManager>();
        services.AddScoped<IQuestionManager, QuestionManager>();

        services.AddSingleton(_aiConfig);
        services.AddScoped<IAiManager>(provider => new AiManagerLogger(
            new TestAiManager(new TestAiManagerConfig {
                IsAllowed = true,
                Alternative = "Please rephrase your idea in a respectful way."
            }),
            new Mock<IAuditRepository>().Object
        ));

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        DataSeeder.Seed(dbContext);
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

    private sealed class TestAiManager(TestAiManagerConfig config) : IAiManager
    {
        public Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null)
        {
            try
            {
                return Task.FromResult(config.Alternative);
            }
            catch (Exception exception)
            {
                return Task.FromException<string>(exception);
            }
        }

        public Task<ModerationDecision> ModerateContent(string content)
        {
            try
            {
                return Task.FromResult(new ModerationDecision
                {
                    IsAllowed = config.IsAllowed
                });
            }
            catch (Exception exception)
            {
                return Task.FromException<ModerationDecision>(exception);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType, object serviceKey = null)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class TestAiManagerConfig
    {
        public bool IsAllowed { get; set; } = true;
        public string Alternative { get; set; } = "Please rephrase your idea in a respectful way.";
    }
}


