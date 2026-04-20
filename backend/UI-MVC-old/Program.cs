using System.ComponentModel;
using Conversey.BL.Administration;
using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Ideation;
using Conversey.BL.Survey;
using Conversey.DAL;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Conversey.DAL.Subplatform.Ai;
using Conversey.DAL.Survey;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.UI_MVC.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<WorkspaceContext>();
builder.Services.AddTransient(p => p.GetRequiredService<WorkspaceContext>().Workspace);
builder.Services.AddScoped<WorkspaceMiddleware>();

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

// MVC 
builder.Services.AddControllersWithViews();


// Add repositories
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();

// Add managers
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();

builder.Services.AddHttpClient("MistralAPI", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri("https://api.mistral.ai/v1/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var apiKey = config["AI:Mistral:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }
});

builder.Services.AddScoped<IAiManager>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var providerName = (config["AI:Provider"] ?? "Noop").Trim();

    if (providerName.Equals("Noop", StringComparison.OrdinalIgnoreCase))
    {
        return new NoopAiManager();
    }

    if (providerName.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
    {
        var apiKey = config["AI:Mistral:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("AI provider is set to Mistral but AI:Mistral:ApiKey is missing.");
        }

        var aiConfig = new AiManagerConfig
        {
            ApiKey = apiKey,
            CompletionsModel = config["AI:Mistral:CompletionsModel"] ?? "mistral-small-latest",
            ModerationModel = config["AI:Mistral:ModerationModel"] ?? "mistral-moderation-latest"
        };

        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        return new MistralAiManager(httpClientFactory.CreateClient("MistralAPI"), aiConfig);
    }

    throw new NotSupportedException($"AI provider '{providerName}' is not supported.");
});

builder.Services.AddDbContext<ConverseyDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
        ?? "Host=localhost;Port=5432;Database=devdb;Username=devuser;Password=devpass")
);

TypeDescriptor.AddAttributes(
    typeof(Slug),
    new TypeConverterAttribute(typeof(SlugTypeConverter))
);

var app = builder.Build();

var resetDatabaseOnStart = builder.Configuration.GetValue<bool>("Database:ResetOnStart");
InitializeDatabase(resetDatabaseOnStart);

app.UseMiddleware<WorkspaceMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("FrontendDev");

app.UseAuthorization();

app.MapControllers();

var webRootPath = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var spaIndexFile = Path.Combine(webRootPath, "index.html");
var useViteDevServer = app.Environment.IsDevelopment() &&
                       builder.Configuration.GetValue("Frontend:UseViteDevServer", true);
var viteHealthCheckedAtUtc = DateTime.MinValue;
var viteIsAvailable = false;

async Task<bool> IsViteDevServerAvailableAsync()
{
    // Cache probe result briefly to avoid a socket check on every page request.
    if (DateTime.UtcNow - viteHealthCheckedAtUtc < TimeSpan.FromSeconds(2))
    {
        return viteIsAvailable;
    }

    try
    {
        using var tcpClient = new System.Net.Sockets.TcpClient();
        var connectTask = tcpClient.ConnectAsync("127.0.0.1", 5173);
        var timeoutTask = Task.Delay(250);
        var completedTask = await Task.WhenAny(connectTask, timeoutTask);

        viteIsAvailable = completedTask == connectTask && tcpClient.Connected;
    }
    catch
    {
        viteIsAvailable = false;
    }

    viteHealthCheckedAtUtc = DateTime.UtcNow;
    return viteIsAvailable;
}

app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    if (useViteDevServer && await IsViteDevServerAvailableAsync())
    {
        var viteTarget = $"http://localhost:5173{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(viteTarget);
        return;
    }

    if (!File.Exists(spaIndexFile))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(spaIndexFile);
});

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
