using Conversey.BL.Domain.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Conversey.DAL.Subplatform.Ai;

#region ProjectAiPromptOverrideConfig
public class ProjectAiPromptOverrideConfig : IEntityTypeConfiguration<ProjectAiPromptOverride>
{
    public void Configure(EntityTypeBuilder<ProjectAiPromptOverride> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.ProjectId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.PromptName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.UserPromptTemplate)
            .IsRequired();

        builder.HasIndex(o => new { o.ProjectId, o.PromptName })
            .IsUnique();
    }
}
#endregion

public class ProjectPromptRepository : IProjectPromptRepository
{
    private readonly ConverseyDbContext _dbContext;

    public ProjectPromptRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProjectAiPromptOverride>> GetOverridesForProjectAsync(string projectId)
    {
        return await _dbContext.ProjectAiPromptOverrides
            .Where(o => o.ProjectId == projectId)
            .ToListAsync();
    }

    public Task<ProjectAiPromptOverride> GetOverrideAsync(string projectId, string promptName)
    {
        return _dbContext.ProjectAiPromptOverrides
            .FirstOrDefaultAsync(o => o.ProjectId == projectId && o.PromptName == promptName);
    }

    public async Task SaveOverrideAsync(ProjectAiPromptOverride projectOverride)
    {
        var existing = await _dbContext.ProjectAiPromptOverrides
            .FirstOrDefaultAsync(o => o.ProjectId == projectOverride.ProjectId && o.PromptName == projectOverride.PromptName);

        if (existing != null)
        {
            existing.UserPromptTemplate = projectOverride.UserPromptTemplate;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            projectOverride.UpdatedAt = DateTime.UtcNow;
            _dbContext.ProjectAiPromptOverrides.Add(projectOverride);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteOverridesForProjectAsync(string projectId)
    {
        var overrides = await _dbContext.ProjectAiPromptOverrides
            .Where(o => o.ProjectId == projectId)
            .ToListAsync();

        if (overrides.Count > 0)
        {
            _dbContext.ProjectAiPromptOverrides.RemoveRange(overrides);
            await _dbContext.SaveChangesAsync();
        }
    }
}
