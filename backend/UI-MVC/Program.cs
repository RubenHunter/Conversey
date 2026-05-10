using System.ComponentModel;
using System.Net.Http.Headers;
using Conversey.BL.Administration;
using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Ideation;
using Conversey.BL.Survey;
using Conversey.DAL;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Conversey.DAL.Subplatform.Ai;
using Conversey.DAL.Survey;
using Microsoft.AspNetCore.HttpOverrides;
using Conversey.UI_MVC.Middleware;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Google.Cloud.Logging.Console;

var builder = WebApplication.CreateBuilder(args);

// Configure Google Cloud Logging in Production
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddGoogleCloudConsole();
}

// FORCEER WEBROOT VOOR PRODUCTIE (DOCKER-VRIENDELIJK)
string webRoot = builder.Environment.IsDevelopment() ? "wwwroot" : "/app/wwwroot";
if (!builder.Environment.IsDevelopment() && !Directory.Exists(webRoot))
{
    // Fallback naar de huidige map als /app/wwwroot niet bestaat
    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
}
builder.WebHost.UseWebRoot(webRoot);
Console.WriteLine($"--- ACTIVE WEBROOT: {webRoot} ---");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure Forwarded Headers for Google Cloud Load Balancer
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddViteServices(options => {
    options.Server.AutoRun = false;
});

// TOTAL RECOVERY MANIFEST LOADER (Nuclear Search)
var viteManifest = new Dictionary<string, string>();
string? manifestPath = null;

try 
{
    // Scrol door alle JSON bestanden om de boosdoener te vinden
    var jsonFiles = Directory.GetFiles("/app", "*.json", SearchOption.AllDirectories);
    foreach (var file in jsonFiles)
    {
        Console.WriteLine($"JSON FOUND: {file}");
        if (file.EndsWith("manifest.json"))
        {
            manifestPath = file;
            break;
        }
    }
} catch { }

if (manifestPath != null)
{
    try 
    {
        var json = File.ReadAllText(manifestPath);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Value.TryGetProperty("file", out var fileProp))
            {
                viteManifest[property.Name] = fileProp.GetString() ?? "";
            }
            if (property.Value.TryGetProperty("css", out var cssArray) && cssArray.ValueKind == System.Text.Json.JsonValueKind.Array && cssArray.GetArrayLength() > 0)
            {
                // Create a synthetic key for the CSS file based on the TS filename
                var cssKey = property.Name.Replace(".ts", ".css");
                viteManifest[cssKey] = cssArray[0].GetString() ?? "";
            }
        }
        Console.WriteLine($"--- SUCCESS: MANIFEST LOADED FROM {manifestPath} ---");
    } catch { }
}
else 
{
    Console.WriteLine("--- CRITICAL: NO manifest.json FOUND ANYWHERE IN /app ---");
}
builder.Services.AddSingleton(viteManifest);

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

builder.Services.AddDbContext<ConverseyDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
        ?? "Host=localhost;Port=5432;Database=devdb;Username=devuser;Password=devpass")
);

// Add health checks for Docker
builder.Services.AddHealthChecks();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
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
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
        policy.AddRequirements(new WorkspaceAdminRequirement());
    });
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

    if (providerName.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
    {
        var apiKey = config["AI:Mistral:ApiKey"];
        var aiConfig = new AiManagerConfig
        {
            ApiKey = apiKey,
            CompletionsModel = config["AI:Mistral:CompletionsModel"] ?? "mistral-small-latest"
        };
        var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
        return new MistralAiManager(httpClientFactory.CreateClient("MistralAPI"), aiConfig);
    }
    return new NoopAiManager();
});

builder.Services.AddScoped<WorkspaceContext>();
builder.Services.AddTransient(p => p.GetRequiredService<WorkspaceContext>().CurrentWorkspace);
builder.Services.AddScoped<WorkspaceMiddleware>();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceAdminHandler>();

TypeDescriptor.AddAttributes(
    typeof(Slug),
    new TypeConverterAttribute(typeof(SlugTypeConverter))
);

var app = builder.Build();

app.UseForwardedHeaders();

InitializeDatabase(builder.Configuration.GetValue<bool>("Database:ResetOnStart"));

if (!app.Environment.IsDevelopment())
{
    // TIJDELIJK AANGEZET OM DE 500 ERROR OP /LOGIN TE ZIEN
    app.UseDeveloperExceptionPage();
    // app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles(); 

app.UseMiddleware<WorkspaceMiddleware>();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("--- SERVER STARTED SUCCESSFULLY ---");

if (app.Environment.IsDevelopment())
{
    app.UseWebSockets();
    app.UseViteDevelopmentServer(useMiddleware: false);
}

app.Run();

void InitializeDatabase(bool drop)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbCtx = services.GetRequiredService<ConverseyDbContext>();
        if (drop) dbCtx.Database.EnsureDeleted();
        dbCtx.Database.EnsureCreated();
        SeedIdentity(services.GetRequiredService<UserManager<ApplicationUser>>(), services.GetRequiredService<RoleManager<IdentityRole>>());
        if (!dbCtx.Workspaces.Any()) DataSeeder.Seed(dbCtx);
    }
}

void SeedIdentity(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    if (!roleManager.RoleExistsAsync("Admin").Result) roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
    EnsureSeedUser(userManager, "admin@hogeschool.nova.be", "hogeschool-nova");
}

void EnsureSeedUser(UserManager<ApplicationUser> userManager, string email, string workspaceId)
{
    var user = userManager.FindByEmailAsync(email).Result;
    if (user == null)
    {
        user = new ApplicationUser { Email = email, UserName = email, EmailConfirmed = true, WorkspaceId = Slug.FromName(workspaceId) };
        userManager.CreateAsync(user, "Test123!").Wait();
    }
    if (!userManager.IsInRoleAsync(user, "Admin").Result) userManager.AddToRoleAsync(user, "Admin").Wait();
}
