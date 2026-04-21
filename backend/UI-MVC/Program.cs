using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Conversey.BL.Administration;
using Microsoft.AspNetCore.WebSockets;
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
using Conversey.BL.Domain.Common;

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

// Speech service
builder.Services.AddScoped<Conversey.BL.Speech.IMistralSpeechService>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("MistralAPI");
    return new Conversey.BL.Speech.MistralSpeechService(httpClient, builder.Configuration["AI:Mistral:ApiKey"] ?? "");
});

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

app.UseWebSockets();

app.UseAuthorization();

app.MapControllers();

// WebSocket endpoint for real-time speech-to-text — transparent proxy to Mistral.
app.Map("/ws/speech/transcribe", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    var apiKey = app.Configuration["AI:Mistral:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("API key not configured");
        return;
    }

    try
    {
        var model = context.Request.Query["model"].FirstOrDefault() ?? "voxtral-mini-transcribe-realtime-2602";
        var language = context.Request.Query["language"].FirstOrDefault() ?? "nl";
        
        using var clientWs = await context.WebSockets.AcceptWebSocketAsync();
        app.Logger.LogInformation("[STT] Client WebSocket connected");

        using var mistralWs = new ClientWebSocket();
        // ClientWebSocket.Options.SetRequestHeader sends the header during the HTTP upgrade handshake
        mistralWs.Options.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        var mistralWsUrl = $"wss://api.mistral.ai/v1/audio/transcriptions?model={Uri.EscapeDataString(model)}&language={Uri.EscapeDataString(language)}";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            await mistralWs.ConnectAsync(new Uri(mistralWsUrl), cts.Token);
            app.Logger.LogInformation("[STT] Connected to Mistral WebSocket");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "[STT] Failed to connect to Mistral WebSocket");
            await clientWs.CloseAsync(WebSocketCloseStatus.InternalServerError, "Mistral connection failed", CancellationToken.None);
            return;
        }
        
        // Send start message
        var startMessage = new { type = "start", model = model, language = language };
        var startBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(startMessage));
        await mistralWs.SendAsync(new ArraySegment<byte>(startBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        app.Logger.LogInformation("Sent start message to Mistral");
        
        // Forward messages bidirectionally, collecting full frames before forwarding
        var clientToMistral = Task.Run(async () =>
        {
            var buffer = new byte[65536];
            try
            {
                while (clientWs.State == WebSocketState.Open && mistralWs.State == WebSocketState.Open)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await clientWs.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.Count > 0) ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await mistralWs.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                        break;
                    }
                    await mistralWs.SendAsync(ms.ToArray(), result.MessageType, true, CancellationToken.None);
                }
            }
            catch
            {
                if (mistralWs.State == WebSocketState.Open)
                    await mistralWs.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error", CancellationToken.None);
            }
        });

        var mistralToClient = Task.Run(async () =>
        {
            var buffer = new byte[65536];
            try
            {
                while (clientWs.State == WebSocketState.Open && mistralWs.State == WebSocketState.Open)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await mistralWs.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.Count > 0) ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await clientWs.CloseAsync(WebSocketCloseStatus.NormalClosure, "Mistral closed", CancellationToken.None);
                        break;
                    }
                    await clientWs.SendAsync(ms.ToArray(), result.MessageType, true, CancellationToken.None);
                }
            }
            catch
            {
                if (clientWs.State == WebSocketState.Open)
                    await clientWs.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error", CancellationToken.None);
            }
        });

        await Task.WhenAll(clientToMistral, mistralToClient);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("Error: " + ex.Message);
    }
});

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
