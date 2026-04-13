using Conversey.BL.Subplatform;
using System.Net.Http.Headers;
using Conversey.BL.Ai;
using Conversey.BL.Ai.Clients.Mistral;
using Conversey.BL.Ai.Managers;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.BL.Subplatform.Survey.Questions;
using Conversey.DAL;
using Conversey.DAL.Subplatform;
using Conversey.DAL.Subplatform.Ai;
using Conversey.DAL.Subplatform.Survey;
using Conversey.DAL.Subplatform.Survey.Ideas;
using Conversey.DAL.Subplatform.Survey.Questions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add repositories
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IPromptRepository, PromptRepository>();

// Add managers
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();

// Add services
builder.Services.AddScoped<PromptManager>();

// Registreer IAiManager met de API-sleutel en modelnaam
builder.Services.AddHttpClient("MistralAPI", client =>
{
    client.BaseAddress = new Uri("https://api.mistral.ai/v1/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Register Mistral client
builder.Services.AddScoped<IMistralClient>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var apiKey = config["AI:Models:ApiKey"];
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("MistralAPI");
    return new MistralHttpClient(httpClient, apiKey);
});

builder.Services.Configure<AiManagerConfig>(builder.Configuration.GetSection($"AI:{builder.Configuration["AI:Provider"]}"));

builder.Services.AddScoped<IAiManager>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var providerName = config["AI:Provider"]; // Bijv. "Mistral", "Azure", "Ollama"

    var aiService = providerName switch
    {
        "Mistral" => new MistralAiManager(
            provider.GetRequiredService<IMistralClient>(),
            provider.GetRequiredService<AiManagerConfig>()
        ),
        /*
        "Azure" => new AzureAIService(
            provider.GetRequiredService<HttpClient>(),
            provider.GetRequiredService<IConfiguration>()
        ),
        "Ollama" => new OllamaAIService(
            provider.GetRequiredService<HttpClient>(),
            provider.GetRequiredService<IConfiguration>()
        ),
        */
        _ => throw new NotSupportedException($"Provider '{providerName}' is niet ondersteund.")
    };

    return new AiManagerLogger(aiService, provider.GetRequiredService<IAuditRepository>());
});


/*builder.Services.AddScoped<IAiManager>(provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient("MistralAPI");
    var config = provider.GetRequiredService<IConfiguration>();
    
    var apiKey = config["AI:ApiKey"];
    var modelName = config["AI:Model"] ?? "mistral-small-latest";
    var moderationModel = config["AI:ModerationModel"] ?? "mistral-moderation-latest";

    return new MistralAiService(new MistralHttpClient(httpClient, apiKey), new AiManagerConfig
    {
        ApiKey = apiKey,
        CompletionsModel = modelName,
        ModerationModel = moderationModel
    });
});*/

builder.Services.AddDbContext<ConverseyDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=devdb;Username=devuser;Password=devpass")
);


var app = builder.Build();

var resetDatabaseOnStart = builder.Configuration.GetValue<bool>("Database:ResetOnStart");
InitializeDatabase(resetDatabaseOnStart);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("FrontendDev");

app.UseAuthorization();

app.MapControllers();

app.Run();

void InitializeDatabase(bool drop)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbCtx = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        if (!dbCtx.CreateDatabase(drop)) return;
        DataSeeder.Seed(dbCtx);
    }
}
