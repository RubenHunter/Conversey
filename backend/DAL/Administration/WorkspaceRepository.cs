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

    public Workspace ReadWorkspaceBySlug(Slug slug)
    {
        return _context.Workspaces.SingleOrDefault(w => w.Id == slug);
    }

    public Workspace ReadWorkspaceById(Slug id)
    {
        return _context.Workspaces.SingleOrDefault(w => w.Id == id);
    }


    public void CreateWorkspace(Workspace workspace)
    {
        _context.Workspaces.Add(workspace);
        _context.SaveChanges();
    }

    public void UpdateWorkspace(Workspace updatedWorkspace)
    {
        _context.Workspaces.Update(updatedWorkspace);
        _context.SaveChanges();
    }

    public void DeleteWorkspace(Workspace workspace)
    {
        _context.Workspaces.Remove(workspace);
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
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        #endregion

    }
}
#endregion
