using Conversey.BL;
using Conversey.DAL;
using Conversey.DAL.EF;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Use correct implementation of IRepository
var repoType = builder.Configuration["Repository:Type"];

if (repoType == "InMemory")
{
    builder.Services.AddScoped<IWorkspaceRepository, InMemoryWorkspaceRepository>();
}
else if (repoType == "Postgres")
{
    builder.Services.AddScoped<IWorkspaceRepository, WorkspaceWorkspaceRepository>();
}
else
{
    throw new Exception($"Unknown repository type: {repoType}");
}

builder.Services.AddScoped<IManager, Manager>();

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
    if (repoType == "Postgres")
    {
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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();