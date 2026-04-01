using Conversey.BL.Subplatform;
using System.Net.Http.Headers;
using Conversey.BL.Ai;
using Conversey.BL.Subplatform.Survey;
using Conversey.BL.Subplatform.Survey.Ideation;
using Conversey.BL.Subplatform.Survey.Questions;
using Conversey.DAL;
using Conversey.DAL.Subplatform;
using Conversey.DAL.Subplatform.Survey;
using Conversey.DAL.Subplatform.Survey.Ideas;
using Conversey.DAL.Subplatform.Survey.Questions;
using Microsoft.EntityFrameworkCore;
using Google.Cloud.SecretManager.V1;

var builder = WebApplication.CreateBuilder(args);

// --- 1. GOOGLE SECRET MANAGER INTEGRATIE ---
Console.WriteLine($"🔍 Environment: {builder.Environment.EnvironmentName}");

if (builder.Environment.IsProduction())
{
    try 
    {
        Console.WriteLine("☁️ Attempting to load secrets from Google Cloud...");
        var client = SecretManagerServiceClient.Create();
        string projectId = "ip1-mvp-project";

        var secrets = client.ListSecrets(new ListSecretsRequest { Parent = $"projects/{projectId}" });

        foreach (var secret in secrets)
        {
            string secretId = secret.SecretName.SecretId;
            string versionName = $"projects/{projectId}/secrets/{secretId}/versions/latest";
            
            var result = client.AccessSecretVersion(versionName);
            string value = result.Payload.Data.ToStringUtf8();

            string configKey = secretId.Replace("__", ":");
            builder.Configuration[configKey] = value;
            
            Console.WriteLine($"✅ Loaded: {configKey}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Secret Manager Error: {ex.Message}");
    }
}

// --- 2. SERVICES CONFIGURATIE ---
builder.Services.AddControllers();

// CORS Policy: We zetten deze op "AllowAny" om poort-conflicten (5173 vs 5231) op te lossen
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Repositories
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// Managers
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();

// AI Manager
builder.Services.AddHttpClient("MistralAPI", client =>
{
    client.BaseAddress = new Uri("https://api.mistral.ai/v1/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<IAiManager>(provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient("MistralAPI");
    var config = provider.GetRequiredService<IConfiguration>();
    
    var apiKey = config["AI:ApiKey"];
    var modelName = config["AI:Model"] ?? "mistral-small-latest";
    var moderationModel = config["AI:ModerationModel"] ?? "mistral-moderation-latest";

    if (string.IsNullOrEmpty(apiKey) && builder.Environment.IsProduction())
    {
        Console.WriteLine("⚠️ WARNING: Mistral API Key is empty.");
    }

    return new MistralAiManager(httpClient, apiKey, modelName, moderationModel);
});

// Database
builder.Services.AddDbContext<ConverseyDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// --- 3. MIDDLEWARE PIPELINE ---

// BELANGRIJK: CORS moet als een van de eerste komen
app.UseCors("FrontendDev");

var resetDatabaseOnStart = builder.Configuration.GetValue<bool>("Database:ResetOnStart");
InitializeDatabase(resetDatabaseOnStart);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // HTTPS Redirection uitgezet voor Docker/VM omgeving zonder SSL certificaat
    // app.UseHttpsRedirection(); 
}

app.UseAuthorization();
app.MapControllers();

app.Run();

// --- 4. DATABASE INITIALISATIE ---
void InitializeDatabase(bool drop)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbCtx = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
        try {
            if (!dbCtx.CreateDatabase(drop)) return;
            DataSeeder.Seed(dbCtx);
            Console.WriteLine("✅ Database initialized and seeded.");
        }
        catch (Exception ex) {
            Console.WriteLine($"❌ DB Init Error: {ex.Message}");
        }
    }
}
