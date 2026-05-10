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

// Configure Google Cloud Logging first
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddGoogleCloudConsole();
}

// DYNAMISCHE WEBROOT DETECTIE
var webRootPath = builder.Environment.ContentRootPath;
if (Directory.Exists("/app/wwwroot")) webRootPath = "/app/wwwroot";
else if (Directory.Exists("wwwroot")) webRootPath = Path.GetFullPath("wwwroot");

builder.WebHost.UseWebRoot(webRootPath);
Console.WriteLine($"--- ACTIVE WEBROOT: {webRootPath} ---");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/login");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "/logout");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/AccessDenied", "/access-denied");
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddViteServices(options => {
    options.Server.AutoRun = false;
});

// NUCLEAR FILE LISTING (For debugging only)
try 
{
    Console.WriteLine("--- LISTING FILES IN /app ---");
    var files = Directory.GetFiles("/app", "*.*", SearchOption.AllDirectories).Take(50);
    foreach (var f in files) Console.WriteLine($"FILE: {f}");
} catch { }

// ROBUUST MANIFEST LOADER
var viteManifest = new Dictionary<string, string>();
string? manifestFile = null;
try 
{
    var candidates = Directory.GetFiles("/app", "manifest.json", SearchOption.AllDirectories);
    if (candidates.Length > 0) manifestFile = candidates[0];
} catch { }

if (manifestFile != null)
{
    try 
    {
        var json = File.ReadAllText(manifestFile);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Value.TryGetProperty("file", out var fileProp))
            {
                viteManifest[property.Name] = fileProp.GetString() ?? "";
            }
        }
        Console.WriteLine($"--- SUCCESS: MANIFEST LOADED FROM {manifestFile} ---");
    } catch { }
}
builder.Services.AddSingleton(viteManifest);

// Repositories & Managers
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();

builder.Services.AddDbContext<ConverseyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default") ?? "Host=localhost;Database=devdb;Username=devuser;Password=devpass")
);

builder.Services.AddHealthChecks();
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 6;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ConverseyDbContext>();

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.LogoutPath = "/logout";
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options => {
    options.AddPolicy(WorkspaceAdminPolicy.Name, policy => {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
        policy.AddRequirements(new WorkspaceAdminRequirement());
    });
});

builder.Services.AddHttpClient("MistralAPI", (sp, client) => {
    var config = sp.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri("https://api.mistral.ai/v1/");
    var apiKey = config["AI:Mistral:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
});

builder.Services.AddScoped<IAiManager>(provider => {
    var config = provider.GetRequiredService<IConfiguration>();
    var providerName = (config["AI:Provider"] ?? "Noop").Trim();
    if (providerName.Equals("Mistral", StringComparison.OrdinalIgnoreCase)) {
        var aiConfig = new AiManagerConfig { ApiKey = config["AI:Mistral:ApiKey"], CompletionsModel = config["AI:Mistral:CompletionsModel"] ?? "mistral-small-latest" };
        return new MistralAiManager(provider.GetRequiredService<IHttpClientFactory>().CreateClient("MistralAPI"), aiConfig);
    }
    return new NoopAiManager();
});

builder.Services.AddScoped<WorkspaceContext>();
builder.Services.AddTransient(p => p.GetRequiredService<WorkspaceContext>().CurrentWorkspace);
builder.Services.AddScoped<WorkspaceMiddleware>();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceAdminHandler>();

TypeDescriptor.AddAttributes(typeof(Slug), new TypeConverterAttribute(typeof(SlugTypeConverter)));

var app = builder.Build();
app.UseForwardedHeaders();

InitializeDatabase(builder.Configuration.GetValue<bool>("Database:ResetOnStart"));

if (!app.Environment.IsDevelopment()) {
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
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void InitializeDatabase(bool drop) {
    using var scope = app.Services.CreateScope();
    var dbCtx = scope.ServiceProvider.GetRequiredService<ConverseyDbContext>();
    if (drop) dbCtx.Database.EnsureDeleted();
    dbCtx.Database.EnsureCreated();
    SeedIdentity(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());
    if (!dbCtx.Workspaces.Any()) DataSeeder.Seed(dbCtx);
}

void SeedIdentity(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) {
    if (!roleManager.RoleExistsAsync("Admin").Result) roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
    EnsureSeedUser(userManager, "admin@hogeschool.nova.be", "hogeschool-nova");
}

void EnsureSeedUser(UserManager<ApplicationUser> userManager, string email, string workspaceId) {
    var user = userManager.FindByEmailAsync(email).Result;
    if (user == null) {
        user = new ApplicationUser { Email = email, UserName = email, EmailConfirmed = true, WorkspaceId = Slug.FromName(workspaceId) };
        userManager.CreateAsync(user, "Test123!").Wait();
    }
    if (!userManager.IsInRoleAsync(user, "Admin").Result) userManager.AddToRoleAsync(user, "Admin").Wait();
}
