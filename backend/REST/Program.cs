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

// Add managers
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();

// Registreer IAiManager met de API-sleutel en modelnaam
builder.Services.AddHttpClient("MistralAPI", client =>
{
    client.BaseAddress = new Uri("https://api.mistral.ai/v1/"); // URL de base
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<IAiManager>(provider =>
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient("MistralAPI");
    var config = provider.GetRequiredService<IConfiguration>();
    
    var apiKey = config["AI:ApiKey"];
    var modelName = config["AI:Model"] ?? "mistral-small-latest";
    var moderationModel = config["AI:ModerationModel"] ?? "mistral-moderation-latest";

    return new MistralAiManager(httpClient, apiKey, modelName, moderationModel);
});

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
