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

// --- GOOGLE SECRET MANAGER INTEGRATIE ---
// --- DEBUG GOOGLE SECRETS ---
Console.WriteLine($"🔍 Environment: {builder.Environment.EnvironmentName}");

if (builder.Environment.IsProduction())
{
    try 
    {
        Console.WriteLine("☁️ Attempting to load secrets from Google Cloud...");
        var client = SecretManagerServiceClient.Create();
        string projectId = "ip1-mvp-project";

        var secrets = client.ListSecrets(new ListSecretsRequest { Parent = $"projects/{projectId}" });

        bool foundAny = false;
        foreach (var secret in secrets)
        {
            foundAny = true;
            string secretId = secret.SecretName.SecretId;
            string versionName = $"projects/{projectId}/secrets/{secretId}/versions/latest";
            
            var result = client.AccessSecretVersion(versionName);
            string value = result.Payload.Data.ToStringUtf8();

            string configKey = secretId.Replace("__", ":");
            builder.Configuration[configKey] = value;
            
            Console.WriteLine($"✅ Loaded: {configKey}");
        }
        
        if (!foundAny) Console.WriteLine("⚠️ No secrets found in this GCP project.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Secret Manager Error: {ex.Message}");
    }
}
else 
{
    Console.WriteLine("ℹ️ Skipping Secret Manager because Environment is NOT Production.");
}
// -----------------------------

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add repositories
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// Add managers
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();

// Registreer IAiManager met de API-sleutel en modelnaam
builder.Services.AddHttpClient("MistralAPI", client =>
{
    client.BaseAddress = new Uri("https://api.mistral.ai/v1/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<IAiManager>(provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient("MistralAPI");
    var config = provider.GetRequiredService<IConfiguration>();
    
    // Deze waarden worden in Production automatisch uit Secret Manager gehaald
    // (In GCP Secret Manager moet de naam "AI__ApiKey" zijn)
    var apiKey = config["AI:ApiKey"];
    var modelName = config["AI:Model"] ?? "mistral-small-latest";
    var moderationModel = config["AI:ModerationModel"] ?? "mistral-moderation-latest";

    // Debug check voor de Deployer
    if (string.IsNullOrEmpty(apiKey) && builder.Environment.IsProduction())
    {
        throw new Exception("CRITICAL: Mistral API Key not found in Secret Manager (checked for name 'AI__ApiKey')");
    }

    return new MistralAiManager(httpClient, apiKey, modelName, moderationModel);
});

// DbContext configuratie
builder.Services.AddDbContext<ConverseyDbContext>(options =>
   options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
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
