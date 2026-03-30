using Conversey.BL.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Subplatform;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.BL.Subplatform.Survey.Questions;
using Conversey.DAL;
using Conversey.DAL.Subplatform;
using Conversey.DAL.Subplatform.Survey;
using Conversey.DAL.Subplatform.Survey.Ideas;
using Conversey.DAL.Subplatform.Survey.Questions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<IAiManager, TestAiManager>();

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

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _connection.Dispose();
    }

    private sealed class TestAiManager : IAiManager
    {
        public string GenerateAiAlternative(string prompt)
        {
            return "Please rephrase your idea in a respectful way.";
        }

        public ModerationDecision ModerateContent(string ideaDescription)
        {
            return new ModerationDecision
            {
                IsAllowed = true
            };
        }
    }
}


