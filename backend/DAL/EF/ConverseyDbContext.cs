using System.Diagnostics;
using Conversey.BL.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Conversey.DAL.EF;


public class ConverseyDbContext : DbContext
{
    public ConverseyDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Conversey
        modelBuilder.Entity<BL.Domain.Entities.Identity.Conversey>()
            .HasKey(c => c.Id);
        
        modelBuilder.Entity<BL.Domain.Entities.Identity.Conversey>()
            .HasMany(c => c.Workspaces)
            .WithOne()
            .HasForeignKey("ConverseyId")
            .IsRequired(false);

        
        
        // Workspace
        modelBuilder.Entity<Workspace>()
            .HasKey(w => w.Id);

        // Workspace 1-* Project
        modelBuilder.Entity<Workspace>()
            .HasMany(w => w.Projects);
        
        modelBuilder.Entity<Workspace>()
            .Property(w => w.Name)
            .IsRequired();
       
        
        
    }
    
    public DbSet<Workspace> Workspaces { get; set; }

    public bool CreateDatabase(bool resetDatabse)
    {
        if (resetDatabse)
        {
            Database.EnsureDeleted();
        }
        
        return Database.EnsureCreated();
    }
}