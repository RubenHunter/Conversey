#nullable enable
using System.ComponentModel;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
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

// FORCEER WEBROOT VOOR PRODUCTIE (DOCKER-VRIENDELIJK)
string webRoot = "wwwroot";
if (!Directory.Exists(webRoot))
{
    // Fallback naar de absolute map als relatieve wwwroot niet gevonden wordt
    webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = webRoot
});

Console.WriteLine($"--- ACTIVE WEBROOT: {builder.Environment.WebRootPath} ---");

// Configure Google Cloud Logging in Production
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddGoogleCloudConsole();
}

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Persist Data Protection keys so antiforgery tokens survive server restarts and work across multiple instances
// This fixes HTTP 400/500 errors in load-balanced cloud environments
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ConverseyDbContext>()
    .SetApplicationName("Conversey");

// Ensure antiforgery cookies work across subdomains
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "conversey-af";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

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
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("Default") 
                      ?? builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Host=localhost;Port=5432;Database=devdb;Username=devuser;Password=devpass";

builder.Services.AddDbContext<ConverseyDbContext>(options =>
    options.UseNpgsql(connectionString)
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
    app.UseExceptionHandler("/Home/Error");
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

app.MapGet("/api/admin/nuke-db-and-reseed", (string code) => {
    if (code != "conversey123") return "Unauthorized";
    InitializeDatabase(true);
    return "Database has been nuked and reseeded with the new English translations!";
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
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        SeedIdentity(userManager, roleManager);
        if (created)
        {
            DataSeeder.Seed(dbCtx);
        }
    }
}

void SeedIdentity(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Create roles if they don't exist
    if (!roleManager.RoleExistsAsync("User").Result)
    {
        roleManager.CreateAsync(new IdentityRole("User")).Wait();
    }
    if (!roleManager.RoleExistsAsync("Admin").Result)
    {
        roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
    }

    EnsureSeedUser(userManager, "admin@hogeschool.nova.be", "hogeschool-nova");
    EnsureSeedUser(userManager, "admin@stad.linden.be", "stad-linden");
}

void EnsureSeedUser(UserManager<ApplicationUser> userManager, string email, string workspaceId)
{
    var normalizedWorkspaceId = Slug.FromName(workspaceId);
    var user = userManager.FindByEmailAsync(email).Result;
    if (user == null)
    {
        user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            WorkspaceId = normalizedWorkspaceId
        };
        userManager.CreateAsync(user, "Test123!").Wait();
    }
    else
    {
        var changed = false;
        if (user.UserName != email)
        {
            user.UserName = email;
            changed = true;
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            changed = true;
        }
        
        if (user.WorkspaceId != normalizedWorkspaceId)
        {
            user.WorkspaceId = normalizedWorkspaceId;
            changed = true;
        }

        if (changed)
        {
            userManager.UpdateAsync(user).Wait();
        }

        if (!userManager.HasPasswordAsync(user).Result)
        {
            userManager.AddPasswordAsync(user, "Test123!").Wait();
        }
    }

    if (!userManager.IsInRoleAsync(user, "Admin").Result)
    {
        userManager.AddToRoleAsync(user, "Admin").Wait();
    }
}
