using System.ComponentModel;
using System.Net.Http.Headers;
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
using Conversey.UI_MVC.RateLimiting;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
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
builder.Services.AddScoped<IPromptRepository, PromptRepository>();
builder.Services.AddScoped<IProviderConfigRepository, ProviderConfigRepository>();
builder.Services.AddScoped<IRateLimitConfigRepository, RateLimitConfigRepository>();
builder.Services.AddScoped<IModerationKeywordRepository, ModerationKeywordRepository>();
builder.Services.AddScoped<ICloudStorageRepository, CloudStorageRepository>();

// Add managers
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();
builder.Services.AddScoped<IAdminManager, AdminManager>();
builder.Services.AddScoped<IAiAdminManager, AiAdminManager>();

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

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dp-keys")));

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
    var appsettingsProviderName = (config["AI:Provider"] ?? "Noop").Trim();

    var providerConfigRepo = provider.GetRequiredService<IProviderConfigRepository>();
    var dbConfigs = Task.Run(() => providerConfigRepo.GetAllConfigsAsync()).GetAwaiter().GetResult();
    var activeDbConfig = dbConfigs.FirstOrDefault(c => c.IsEnabled);

    if (activeDbConfig != null)
    {
        return BuildAiManagerFromDbConfig(provider, activeDbConfig);
    }

    if (appsettingsProviderName.Equals("Noop", StringComparison.OrdinalIgnoreCase))
    {
        var keywordRepo = provider.GetRequiredService<IModerationKeywordRepository>();
        return new NoopAiManager(keywordRepo);
    }

    if (appsettingsProviderName.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
    {
        var apiKey = config["AI:Mistral:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("AI provider is set to Mistral but AI:Mistral:ApiKey is missing.");
        }

        var completionsModel = config["AI:Mistral:CompletionsModel"] ?? "mistral-small-latest";
        var moderationModel = config["AI:Mistral:ModerationModel"] ?? "mistral-moderation-latest";

        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var mistralProvider = new MistralAiProvider(factory.CreateClient("MistralAPI"));
        var promptRepo = provider.GetRequiredService<IPromptRepository>();
        var auditRepo = provider.GetRequiredService<IAuditRepository>();
        var keywordRepo = provider.GetRequiredService<IModerationKeywordRepository>();

        return new AiManager(mistralProvider, promptRepo, auditRepo, keywordRepo, completionsModel, moderationModel);
    }

    throw new NotSupportedException($"AI provider '{appsettingsProviderName}' is not supported.");
});

builder.Services.AddScoped<WorkspaceContext>();
builder.Services.AddTransient(p => p.GetRequiredService<WorkspaceContext>().CurrentWorkspace);
builder.Services.AddSingleton<AdminContext>();
builder.Services.AddTransient(p => p.GetRequiredService<AdminContext>().CurrentAdmin);
builder.Services.AddScoped<WorkspaceMiddleware>();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ConverseyAdminHandler>();

builder.Services.AddSingleton<RateLimitConfigCache>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy<string, AiUserRateLimiterPolicy>("AiFixedPolicy");
    options.AddPolicy<string, AiAdminRateLimiterPolicy>("AiAdminPolicy");
});


TypeDescriptor.AddAttributes(
    typeof(Slug),
    new TypeConverterAttribute(typeof(SlugTypeConverter))
);

var app = builder.Build();

var resetDatabaseOnStart = builder.Configuration.GetValue<bool>("Database:ResetOnStart");
InitializeDatabase(resetDatabaseOnStart);

var rateLimitCache = app.Services.GetRequiredService<RateLimitConfigCache>();
await rateLimitCache.InitializeAsync();

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

app.UseRateLimiter();

// if (app.Environment.IsDevelopment())
// {
//     app.UseCors(viteDevCorsPolicy);
// }

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

static AiManager BuildAiManagerFromDbConfig(IServiceProvider provider, AiProviderConfig config)
{
    if (string.IsNullOrWhiteSpace(config.BaseUrl))
    {
        throw new InvalidOperationException($"AI provider '{config.ProviderName}' is enabled but has no BaseUrl configured.");
    }

    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient();
    httpClient.BaseAddress = new Uri(config.BaseUrl);
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    IAiProvider aiProvider;

    if (config.ProviderName.Equals("Azure", StringComparison.OrdinalIgnoreCase))
    {
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);
        }

        aiProvider = new AzureOpenAiProvider(httpClient, config.CompletionsModel, config.ApiVersion);
    }
    else if (config.ProviderName.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
    {
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        }

        aiProvider = new MistralAiProvider(httpClient);
    }
    else
    {
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        }

        aiProvider = new OpenAiCompatibleProvider(httpClient, config.ProviderName);
    }

    var promptRepo = provider.GetRequiredService<IPromptRepository>();
    var auditRepo = provider.GetRequiredService<IAuditRepository>();
    var keywordRepo = provider.GetRequiredService<IModerationKeywordRepository>();
    var completionsModel = string.IsNullOrWhiteSpace(config.CompletionsModel) ? "mistral-small-latest" : config.CompletionsModel;
    var moderationModel = config.ModerationModel;

    return new AiManager(aiProvider, promptRepo, auditRepo, keywordRepo, completionsModel, moderationModel, config.Temperature);
}
