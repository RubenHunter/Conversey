using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL;

public class ConverseyDbContext : DbContext
{
    
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<Idea> Ideas { get; set; }
    public DbSet<Project> Projects { get; set; }
    
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
            .HasMaxLength(50)
            .HasConversion(
                slug => slug.ToString(),
                str => Slug.FromName(str));
    }
    

    public bool CreateDatabase(bool resetDatabase)
    {
        if (resetDatabase)
        {
            Database.EnsureDeleted();
        }

        return Database.EnsureCreated();
    }
}