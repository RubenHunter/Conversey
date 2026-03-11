using System.Net.Http.Headers;
using Conversey.BL.Ai;
using Conversey.BL.Subplatform;
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

// Use correct implementation of IRepository
var repoType = builder.Configuration["Repository:Type"];

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
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Initialize Development Database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ConverseyDbContext>();
        if (context.Database.EnsureCreated())
        {
            DataSeeder.Seed(context);
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();