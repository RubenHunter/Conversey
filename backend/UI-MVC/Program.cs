using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Conversey.BL.Administration;
using Conversey.BL.Ai;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Ideation;
using Conversey.BL.Survey;
using Conversey.DAL;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Conversey.DAL.Subplatform.Ai;
using Conversey.DAL.Survey;
using Conversey.UI_MVC.Middleware;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
// const string viteDevCorsPolicy = "ViteDevCors";

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/login");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "/logout");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/AccessDenied", "/access-denied");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Manage/ChangePassword", "/change-password");
    });

builder.Services.AddViteServices(options =>
{
	options.Server.Port = 4173;
    options.Server.AutoRun = true;
    options.Server.PackageManager = "pnpm";
});

// Add repositories
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

// Add managers
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();
builder.Services.AddScoped<IAdminManager, AdminManager>();

builder.Services.AddDbContext<ConverseyDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
        ?? "Host=localhost;Port=5432;Database=devdb;Username=devuser;Password=devpass")
);

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ConverseyDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.LogoutPath = "/logout";
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
options.AddPolicy(WorkspaceAdminPolicy.Name, policy =>
        {
            policy.AddRequirements(new WorkspaceAdminRequirement());
        });

        options.AddPolicy(ConverseyAdminPolicy.Name, policy =>
        {
            policy.AddRequirements(new ConverseyAdminRequirement());
        });
});

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy(viteDevCorsPolicy, policy =>
//     {
//         policy.WithOrigins(
//                 "http://localhost:4173",
//                 "https://localhost:4173",
//                 "http://localhost:4180",
//                 "https://localhost:7093")
//             .AllowAnyHeader()
//             .AllowAnyMethod()
//             .AllowCredentials();
//     });
// });

// Speech service
builder.Services.AddSingleton<Conversey.BL.Speech.IMistralVoiceManager, Conversey.BL.Speech.MistralVoiceManager>();
builder.Services.AddScoped<Conversey.BL.Speech.IMistralSpeechManager>(provider =>
{
    var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("MistralAPI");
    var voiceManager = provider.GetRequiredService<Conversey.BL.Speech.IMistralVoiceManager>();
    return new Conversey.BL.Speech.MistralSpeechManager(
        httpClient,
        builder.Configuration["AI:Mistral:ApiKey"] ?? "",
        voiceManager
    );
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
            ModerationModel = config["AI:Mistral:ModerationModel"] ?? "mistral-moderation-latest",
            NudgingMode = config["AI:Nudging:Mode"] ?? "Balanced"
        };

        provider.GetRequiredService<AiManagerConfig>();

        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        return new MistralAiManager(httpClientFactory.CreateClient("MistralAPI"), aiConfig);
    }

    throw new NotSupportedException($"AI provider '{providerName}' is not supported.");
});

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new AiManagerConfig
    {
        ApiKey = config["AI:Mistral:ApiKey"] ?? string.Empty,
        CompletionsModel = config["AI:Mistral:CompletionsModel"] ?? "mistral-small-latest",
        ModerationModel = config["AI:Mistral:ModerationModel"] ?? "mistral-moderation-latest",
        NudgingMode = config["AI:Nudging:Mode"] ?? "Balanced",
    };
});

builder.Services.AddScoped<WorkspaceContext>();
builder.Services.AddTransient(p => p.GetRequiredService<WorkspaceContext>().CurrentWorkspace);
builder.Services.AddScoped<WorkspaceMiddleware>();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ConverseyAdminHandler>();


TypeDescriptor.AddAttributes(
    typeof(Slug),
    new TypeConverterAttribute(typeof(SlugTypeConverter))
);

var app = builder.Build();

var resetDatabaseOnStart = builder.Configuration.GetValue<bool>("Database:ResetOnStart");
InitializeDatabase(resetDatabaseOnStart);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Assets")),
    RequestPath = "/Assets"
});
app.UseMiddleware<WorkspaceMiddleware>();
app.UseRouting();

// if (app.Environment.IsDevelopment())
// {
//     app.UseCors(viteDevCorsPolicy);
// }

app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Project}/{action=Landing}/{id?}")
    .WithStaticAssets();

// Serve the SPA shell for non-file URLs so browser refresh on client routes keeps working.
//app.MapFallbackToController("Index", "Home");

if (app.Environment.IsDevelopment())
{
    app.UseWebSockets();
    app.UseViteDevelopmentServer(useMiddleware: false);
}
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
app.Run();

void InitializeDatabase(bool drop)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbCtx = services.GetRequiredService<ConverseyDbContext>();
        // Create database schema first (including Identity tables)
        var created = dbCtx.CreateDatabase(drop);
        // Then seed Identity and Roles
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (created)
        {
            DataSeeder.Seed(dbCtx);
            SeedIdentity(userManager, roleManager, dbCtx);
        }
    }
}

void SeedIdentity(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ConverseyDbContext dbCtx)
{
    // Create roles if they don't exist
    if (!roleManager.RoleExistsAsync("User").Result)
    {
        roleManager.CreateAsync(new IdentityRole("User")).Wait();
    }
    if (!roleManager.RoleExistsAsync("WorkspaceAdmin").Result)
    {
        roleManager.CreateAsync(new IdentityRole("WorkspaceAdmin")).Wait();
    }
    if (!roleManager.RoleExistsAsync("ConverseyAdmin").Result)
    {
        roleManager.CreateAsync(new IdentityRole("ConverseyAdmin")).Wait();
    }

    // Seed ConverseyAdmin
    EnsureSeedUser(userManager, "admin@conversey.be", "ConverseyAdmin");

    // Seed WorkspaceAdmins
    var hogeschoolNovaWorkspace = dbCtx.Workspaces.FirstOrDefault(w => w.Id == Slug.FromName("hogeschool-nova"));
    var stadLindenWorkspace = dbCtx.Workspaces.FirstOrDefault(w => w.Id == Slug.FromName("stad-linden"));

    if (hogeschoolNovaWorkspace == null)
    {
        hogeschoolNovaWorkspace = new Workspace
        {
            Id = Slug.FromName("hogeschool-nova"),
            Name = "Hogeschool Nova"
        };
        dbCtx.Workspaces.Add(hogeschoolNovaWorkspace);
        dbCtx.SaveChanges();
    }

    if (stadLindenWorkspace == null)
    {
        stadLindenWorkspace = new Workspace
        {
            Id = Slug.FromName("stad-linden"),
            Name = "Stad Linden"
        };
        dbCtx.Workspaces.Add(stadLindenWorkspace);
        dbCtx.SaveChanges();
    }

    EnsureSeedUser(userManager, "admin@hogeschool.nova.be", "WorkspaceAdmin", hogeschoolNovaWorkspace);
    EnsureSeedUser(userManager, "admin@stad.linden.be", "WorkspaceAdmin", stadLindenWorkspace);
}

void EnsureSeedUser(UserManager<IdentityUser> userManager, string email, string role, Workspace workspace = null)
{
    var user = userManager.FindByEmailAsync(email).Result;
    if (user == null)
    {
        if (role == "ConverseyAdmin")
        {
            user = new ConverseyAdminUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };
        }
        else if (role == "WorkspaceAdmin")
        {
            user = new WorkspaceAdminUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                Workspace = workspace
            };
        }
        else
        {
            return;
        }

        userManager.CreateAsync(user, "Test123!").Wait();
    }

    if (!userManager.IsInRoleAsync(user, role).Result)
    {
        userManager.AddToRoleAsync(user, role).Wait();
    }
}
