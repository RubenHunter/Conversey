using Conversey.BL.Domain.Subplatform;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL;


public class ConverseyDbContext : DbContext
{
    public ConverseyDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
        // Workspace
        modelBuilder.Entity<Workspace>()
            .HasKey(w => w.Id);

        // Workspace 1-* Project
        modelBuilder.Entity<Workspace>()
            .HasMany(w => w.Projects);
        
        modelBuilder.Entity<Workspace>()
            .Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(50);


        modelBuilder.Entity<Workspace>()
            .Property(w => w.Slug)
            .IsRequired()
            .HasMaxLength(50);



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