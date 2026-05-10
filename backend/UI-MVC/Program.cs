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

// Configure Forwarded Headers for Google Cloud Load Balancer
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddViteServices();

// Voorkom 500 errors als manifest niet klopt in productie
builder.Services.Configure<ViteOptions>(options => {
    options.Base = "/";
});

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


TypeDescriptor.AddAttributes(
    typeof(Slug),
    new TypeConverterAttribute(typeof(SlugTypeConverter))
);

var app = builder.Build();

app.UseForwardedHeaders();

// Verwijder de nood-redirect, HomeController handelt dit nu af

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapHealthChecks("/health");

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Project}/{action=Landing}/{id?}")
    .WithStaticAssets();

// Verwijder de oude MapGet redirect die we onderaan hadden staan

app.MapGet("/health", () => Results.Ok("Healthy"));

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
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        SeedIdentity(userManager, roleManager);
        
        Console.WriteLine("Checking if database needs seeding...");
        if (!dbCtx.Workspaces.Any())
        {
            Console.WriteLine("Database is empty! Starting DataSeeder...");
            DataSeeder.Seed(dbCtx);
            Console.WriteLine("DataSeeder finished successfully.");
        }
        else
        {
            Console.WriteLine($"Database already contains {dbCtx.Workspaces.Count()} workspaces. Skipping seed.");
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
