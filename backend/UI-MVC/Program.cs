using System.ComponentModel;
using System.Net.Http.Headers;
using Conversey.BL.Administration;
using Conversey.BL.Ai;
using Conversey.BL.Analytics;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Ideation;
using Conversey.BL.Services;
using Conversey.BL.Survey;
using Conversey.DAL;
using Conversey.DAL.Administration;
using Conversey.DAL.Analytics;
using Conversey.DAL.Ideation;
using Conversey.DAL.Subplatform.Ai;
using Conversey.DAL.Survey;
using Conversey.UI_MVC.Middleware;
using Conversey.UI_MVC.Models;
using Conversey.UI_MVC.RateLimiting;
using Conversey.BL.Ai.Speech;
using Conversey.UI_MVC.Resources;
using Conversey.UI_MVC.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);
// const string viteDevCorsPolicy = "ViteDevCors";

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/login");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "/logout");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/AccessDenied", "/access-denied");
    });

builder.Services.AddViteServices(options =>
{
	options.Server.Port = 4173;
    options.Server.AutoRun = builder.Environment.IsDevelopment();
    options.Server.PackageManager = "pnpm";
    options.Manifest = ".vite/manifest.json";
});

// Add repositories
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IPromptRepository, PromptRepository>();
builder.Services.AddScoped<IProjectPromptRepository, ProjectPromptRepository>();
builder.Services.AddScoped<IProviderConfigRepository, ProviderConfigRepository>();
builder.Services.AddScoped<IRateLimitConfigRepository, RateLimitConfigRepository>();
builder.Services.AddScoped<IModerationKeywordRepository, ModerationKeywordRepository>();
builder.Services.AddScoped<ICostLimitRepository, CostLimitRepository>();
builder.Services.AddScoped<IModelPricingRepository, ModelPricingRepository>();
builder.Services.AddScoped<ICloudStorageRepository, CloudStorageRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// Add managers
builder.Services.AddScoped<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddScoped<IProjectManager, ProjectManager>();
builder.Services.AddScoped<IIdeaManager, IdeaManager>();
builder.Services.AddScoped<IQuestionManager, QuestionManager>();
builder.Services.AddScoped<IAdminManager, AdminManager>();
builder.Services.AddScoped<IAiAdminManager, AiAdminManager>();
builder.Services.AddScoped<IAiPricingService, AiPricingService>();
builder.Services.AddScoped<IAnalyticsManager, AnalyticsManager>();
builder.Services.AddScoped<IContactManager, ContactManager>();
builder.Services.AddScoped<IEmailService, GmailEmailService>();

builder.Services.AddDbContext<ConverseyDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
        ?? "Host=localhost;Port=5432;Database=devdb;Username=devuser;Password=devpass")
);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Trust all proxies in the cloud environment (GCLB IPs are dynamic)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ConverseyDbContext>()
    .SetApplicationName("Conversey");

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".Conversey.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 6;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ConverseyDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.LogoutPath = "/logout";
    // Share the auth cookie across all subdomains (e.g. hogeschool-nova.conversey.be)
    options.Cookie.Domain = builder.Environment.IsDevelopment() ? null : ".conversey.be";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// builder.Services.AddDataProtection()
//     .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dp-keys")));

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

        options.AddPolicy(AdminPolicy.Name, policy =>
        {
            policy.AddRequirements(new AdminRequirement());
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

builder.Services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<IAiManager>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var appsettingsProviderName = (config["AI:Provider"] ?? "Noop").Trim();

    var providerConfigRepo = provider.GetRequiredService<IProviderConfigRepository>();
    var dbConfigs = Task.Run(() => providerConfigRepo.GetAllConfigsAsync()).GetAwaiter().GetResult();
    var activeDbConfig = dbConfigs.FirstOrDefault(c => c.IsEnabled);

    IAiManager inner;
    if (activeDbConfig != null)
    {
        inner = BuildAiManagerFromDbConfig(provider, activeDbConfig);
    }
    else if (appsettingsProviderName.Equals("Noop", StringComparison.OrdinalIgnoreCase))
    {
        var keywordRepo = provider.GetRequiredService<IModerationKeywordRepository>();
        inner = new NoopAiManager(keywordRepo);
    }
    else if (appsettingsProviderName.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
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
        var projectPromptRepo = provider.GetRequiredService<IProjectPromptRepository>();
        var auditRepo = provider.GetRequiredService<IAuditRepository>();
        var keywordRepo = provider.GetRequiredService<IModerationKeywordRepository>();
        var pricingService = provider.GetRequiredService<IAiPricingService>();

        inner = new AiManager(mistralProvider, promptRepo, projectPromptRepo, auditRepo, keywordRepo, pricingService, completionsModel, moderationModel);
    }
    else
    {
        throw new NotSupportedException($"AI provider '{appsettingsProviderName}' is not supported.");
    }

    var costLimitRepo = provider.GetRequiredService<ICostLimitRepository>();
    var auditRepoWrap = provider.GetRequiredService<IAuditRepository>();
    var keywordRepoWrap = provider.GetRequiredService<IModerationKeywordRepository>();
    return new CostLimitEnforcingAiManager(inner, costLimitRepo, auditRepoWrap, keywordRepoWrap);
});

builder.Services.AddSingleton<IVoiceManager, MistralVoiceManager>();
builder.Services.AddScoped<ISpeechManager>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var factory = provider.GetRequiredService<IHttpClientFactory>();

    var providerConfigRepo = provider.GetRequiredService<IProviderConfigRepository>();
    var dbConfigs = Task.Run(() => providerConfigRepo.GetAllConfigsAsync()).GetAwaiter().GetResult();
    var speechDbConfig = dbConfigs.FirstOrDefault(c => c.IsEnabled && !string.IsNullOrWhiteSpace(c.SttModel));

    if (speechDbConfig != null)
    {
        return BuildSpeechManagerFromDbConfig(provider, speechDbConfig);
    }

    var speechProvider = (config["AI:Speech:Provider"] ?? config["AI:Provider"] ?? "Noop").Trim();
    var logFactory = provider.GetRequiredService<ILoggerFactory>();

    if (speechProvider.Equals("Noop", StringComparison.OrdinalIgnoreCase))
    {
        return new NoopSpeechManager(logFactory.CreateLogger<NoopSpeechManager>());
    }

    if (speechProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) ||
        speechProvider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
    {
        var openAiKey = config["AI:OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(openAiKey))
        {
            throw new InvalidOperationException("AI:Speech:Provider is set to OpenAI but AI:OpenAI:ApiKey is missing.");
        }

        var sttModel = config["AI:OpenAI:SttModel"] ?? "whisper-1";
        var ttsModel = config["AI:OpenAI:TtsModel"] ?? "tts-1";
        var httpClient = factory.CreateClient("OpenAI");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiKey);
        return new OpenAiSpeechManager(httpClient, logFactory.CreateLogger<OpenAiSpeechManager>(), sttModel, ttsModel);
    }

    return new MistralSpeechManager(
        factory.CreateClient("MistralAPI"),
        provider.GetRequiredService<IVoiceManager>(),
        logFactory.CreateLogger<MistralSpeechManager>());
});

builder.Services.AddScoped<WorkspaceContext>();
builder.Services.AddTransient(p => p.GetRequiredService<WorkspaceContext>().CurrentWorkspace);
builder.Services.AddScoped<AdminContext>();
builder.Services.AddScoped<AdminContextMiddleware>();
builder.Services.AddTransient(p => p.GetRequiredService<AdminContext>().CurrentAdmin);
builder.Services.AddScoped<WorkspaceMiddleware>();
builder.Services.AddScoped<IAdminAccessService, AdminAccessService>();
builder.Services.AddScoped<IAuthorizationHandler, WorkspaceAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ConverseyAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AdminHandler>();
builder.Services.AddScoped<IProjectAccessService, ProjectAccessService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<AdminI18nService>();
builder.Services.AddSingleton<IAdminI18nService>(p => p.GetRequiredService<AdminI18nService>());

builder.Services.AddSingleton<RateLimitConfigCache>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-US", "nl-BE", "fr-BE" };
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
    options.SupportedUICultures = supportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider
    {
        CookieName = ".Conversey.Admin.Culture",
        Options = options
    });
});

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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // Trust all proxies in the cloud environment (GCLB/Cloudflare IPs are dynamic)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = null; // Trust all hops
});

var app = builder.Build();



var resetDatabaseOnStart = builder.Configuration.GetValue<bool>("Database:ResetOnStart");
InitializeDatabase(resetDatabaseOnStart);

var rateLimitCache = app.Services.GetRequiredService<RateLimitConfigCache>();
await rateLimitCache.InitializeAsync();

// UseForwardedHeaders must be placed at the very beginning of the pipeline
app.UseForwardedHeaders();

app.MapGet("/health", () => Results.Ok("Healthy"));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


// Redirect www.conversey.be -> conversey.be to prevent CSRF/cookie domain mismatches
if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var host = context.Request.Host.Host;
        if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
        {
            var canonical = host[4..]; // strip 'www.'
            // Always use https for production redirects to avoid loops with GCLB
            var redirectUrl = $"https://{canonical}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
            context.Response.Redirect(redirectUrl, permanent: true);
            return;
        }
        await next();
    });
}

app.UseStaticFiles(); // Serve files from wwwroot (Vite build output)

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Assets")),
    RequestPath = "/Assets"
});
app.UseMiddleware<WorkspaceMiddleware>();
app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseRequestLocalization();
});
app.UseRouting();

app.UseRateLimiter();

// if (app.Environment.IsDevelopment())
// {
//     app.UseCors(viteDevCorsPolicy);
// }

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AdminContextMiddleware>();

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
        var config = services.GetRequiredService<IConfiguration>();
        
        // Create database schema first (including Identity tables)
        var created = dbCtx.CreateDatabase(drop);

        // Ensure DataProtectionKeys table exists for persistent keys
        dbCtx.Database.ExecuteSqlRaw(
            "CREATE TABLE IF NOT EXISTS \"DataProtectionKeys\" (" +
            "\"Id\" INT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY, " +
            "\"FriendlyName\" TEXT, " +
            "\"Xml\" TEXT);");

        // Schema patches — idempotent, safe to run on every startup.
        // These handle cases where EnsureCreated() produced a schema that is now
        // out of sync with the domain model (e.g. column nullability changes).
        ApplySchemaMigrations(dbCtx);


        if (created)
        {
            DataSeeder.Seed(dbCtx, config);
        }

        // Then seed Identity and Roles (idempotent)
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        SeedIdentity(userManager, roleManager, dbCtx);

        var pricingService = services.GetRequiredService<IAiPricingService>();
        pricingService.RefreshPricingAsync().GetAwaiter().GetResult();
    }
}

void ApplySchemaMigrations(ConverseyDbContext dbCtx)
{
    try
    {
        // Patch: Ideas.Summary was created as NOT NULL but must be nullable
        // (it is AI-generated and not available at idea submission time).
        dbCtx.Database.ExecuteSqlRaw(
            "ALTER TABLE \"Ideas\" ALTER COLUMN \"Summary\" DROP NOT NULL;");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SchemaMigration] ALTER TABLE Ideas.Summary: {ex.Message}");
    }

    try
    {
        // Patch: Add CASCADE delete to Ideas -> Project relationship
        // This allows deleting projects that already have ideas.
        // We drop and recreate the constraint to be sure it has ON DELETE CASCADE.
        dbCtx.Database.ExecuteSqlRaw(@"
            DO $$ 
            BEGIN 
                -- Find and drop the existing FK to Projects if it exists
                IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Ideas_Projects_ProjectId') THEN
                    ALTER TABLE ""Ideas"" DROP CONSTRAINT ""FK_Ideas_Projects_ProjectId"";
                END IF;
                
                -- Recreate with CASCADE
                ALTER TABLE ""Ideas"" ADD CONSTRAINT ""FK_Ideas_Projects_ProjectId"" 
                FOREIGN KEY (""ProjectId"") REFERENCES ""Projects""(""Id"") ON DELETE CASCADE;
            END $$;");
            
        // Patch: Add CASCADE delete to Ideas -> Topic relationship
        dbCtx.Database.ExecuteSqlRaw(@"
            DO $$ 
            BEGIN 
                IF EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_Ideas_Topics_TopicId') THEN
                    ALTER TABLE ""Ideas"" DROP CONSTRAINT ""FK_Ideas_Topics_TopicId"";
                END IF;
                
                ALTER TABLE ""Ideas"" ADD CONSTRAINT ""FK_Ideas_Topics_TopicId"" 
                FOREIGN KEY (""TopicId"") REFERENCES ""Topics""(""Id"") ON DELETE CASCADE;
            END $$;");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SchemaMigration] CASCADE patch: {ex.Message}");
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


    var ipWorkspace = dbCtx.Workspaces.FirstOrDefault(w => w.Id == Slug.FromName("integratie-project-j2"));
    if (ipWorkspace == null)
    {
        ipWorkspace = new Workspace
        {
            Id = Slug.FromName("integratie-project-j2"),
            Name = "Integratie Project J2"
        };
        dbCtx.Workspaces.Add(ipWorkspace);
        dbCtx.SaveChanges();
    }
    EnsureSeedUser(userManager, "admin@integratieproject.be", "WorkspaceAdmin", ipWorkspace);
    // We do this via raw SQL since EF Core doesn't allow changing type of tracked entity easily, 
    // and we want to avoid DeleteAsync due to FK constraints.
    dbCtx.Database.ExecuteSqlRaw(
        "UPDATE \"AspNetUsers\" SET \"Discriminator\" = 'WorkspaceAdminUser', \"WorkspaceId\" = 'stad-linden' " +
        "WHERE \"Email\" = 'admin@stad.linden.be'");
    
    dbCtx.Database.ExecuteSqlRaw(
        "UPDATE \"AspNetUsers\" SET \"Discriminator\" = 'WorkspaceAdminUser', \"WorkspaceId\" = 'hogeschool-nova' " +
        "WHERE \"Email\" = 'admin@hogeschool.nova.be'");

    dbCtx.Database.ExecuteSqlRaw(
        "UPDATE \"AspNetUsers\" SET \"Discriminator\" = 'WorkspaceAdminUser', \"WorkspaceId\" = 'integratie-project-j2' " +
        "WHERE \"Email\" = 'admin@integratieproject.be'");

    dbCtx.Database.ExecuteSqlRaw(
        "UPDATE \"AspNetUsers\" SET \"Discriminator\" = 'ConverseyAdminUser' " +
        "WHERE \"Email\" = 'admin@conversey.be' AND \"Discriminator\" != 'ConverseyAdminUser'");
}

void EnsureSeedUser(UserManager<IdentityUser> userManager, string email, string role, Workspace workspace = null)
{
    var user = userManager.FindByEmailAsync(email).Result;
    
    // 1. Create if missing
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

        var result = userManager.CreateAsync(user, "Test123!").Result;
        if (!result.Succeeded)
        {
            // Log error or throw if critical, but don't crash the whole app if possible
            return;
        }
    }
    else
    {
        // 2. If exists, ensure correct properties and password (DO NOT DELETE due to FKs)
        if (user is WorkspaceAdminUser wau && workspace != null)
        {
            wau.Workspace = workspace;
            userManager.UpdateAsync(wau).Wait();
        }

        // Force password reset to ensure consistency
        var hasPassword = userManager.HasPasswordAsync(user).Result;
        if (hasPassword)
        {
            userManager.RemovePasswordAsync(user).Wait();
        }
        userManager.AddPasswordAsync(user, "Test123!").Wait();
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
    httpClient.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + '/');
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
    var projectPromptRepo = provider.GetRequiredService<IProjectPromptRepository>();
    var auditRepo = provider.GetRequiredService<IAuditRepository>();
    var keywordRepo = provider.GetRequiredService<IModerationKeywordRepository>();
    var pricingService = provider.GetRequiredService<IAiPricingService>();
    var completionsModel = string.IsNullOrWhiteSpace(config.CompletionsModel) ? "mistral-small-latest" : config.CompletionsModel;
    var moderationModel = config.ModerationModel;

    return new AiManager(aiProvider, promptRepo, projectPromptRepo, auditRepo, keywordRepo, pricingService, completionsModel, moderationModel, config.Temperature);
}

static ISpeechManager BuildSpeechManagerFromDbConfig(IServiceProvider provider, AiProviderConfig config)
{
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient();
    httpClient.BaseAddress = new Uri(config.BaseUrl.TrimEnd('/') + '/');
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    if (!string.IsNullOrWhiteSpace(config.ApiKey))
    {
        if (config.ProviderName.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);
        }
        else
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        }
    }

    var sttModel = string.IsNullOrWhiteSpace(config.SttModel) ? "whisper-1" : config.SttModel;
    var ttsModel = string.IsNullOrWhiteSpace(config.TtsModel) ? "tts-1" : config.TtsModel;

    var logFactory = provider.GetRequiredService<ILoggerFactory>();

    if (config.ProviderName.Equals("Mistral", StringComparison.OrdinalIgnoreCase))
    {
        return new MistralSpeechManager(
            httpClient,
            provider.GetRequiredService<IVoiceManager>(),
            logFactory.CreateLogger<MistralSpeechManager>(),
            sttModel,
            ttsModel);
    }

    return new OpenAiSpeechManager(httpClient, logFactory.CreateLogger<OpenAiSpeechManager>(), sttModel, ttsModel);
}
