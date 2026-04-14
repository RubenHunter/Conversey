using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conversey.DAL.Administration;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly ConverseyDbContext _context;

    public WorkspaceRepository(ConverseyDbContext context)
    {
        _context = context;
    }

    public IReadOnlyCollection<Workspace> ReadAllWorkspaces()
    {
        return _context.Workspaces.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<Workspace> ReadAllWorkspacesWithProjects()
    {
        return _context.Workspaces
            .Include(w => w.Projects)
            .ToList().AsReadOnly();
    }

    public Workspace ReadWorkspaceBySlug(Slug slug)
    {
        return _context.Workspaces.SingleOrDefault(w => w.Slug == slug);
    }

    public Workspace ReadWorkspaceBySlugWithProjects(Slug slug)
    {
        return _context.Workspaces
            .Include(w => w.Projects)
            .SingleOrDefault(w => w.Slug == slug);
    }

    public Workspace ReadWorkspaceById(int id)
    {
        return _context.Workspaces.SingleOrDefault(w => w.Id == id);
    }

    public Workspace ReadWorkspaceByIdWithProjects(int id)
    {
        return _context.Workspaces
            .Include(w => w.Projects)
            .SingleOrDefault(w => w.Id == id);
    }

    public void CreateWorkspace(Workspace workspace)
    {
        _context.Workspaces.Add(workspace);
        _context.SaveChanges();
    }
}

#region WorkspaceConfig
public class WorkspaceConfig: IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        #region Properties
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(49);

        builder.Property(w => w.Id)
            .IsRequired()
            .HasMaxLength(49)
            .HasConversion(
                slug => slug.Text,
                str => new Slug { Text = str }
            );
        #endregion



        #region Relations
        // Workspace 0-* Project
        builder.HasMany(w => w.Projects)
            .WithOne(p => p.Workspace)
            .HasForeignKey("WorkspaceId")
            .IsRequired();
        #endregion

    }
}
#endregion
