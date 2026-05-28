#nullable enable
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Ai;

public class CostLimitRepository : ICostLimitRepository
{
    private readonly ConverseyDbContext _dbContext;

    public CostLimitRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AiCostLimit?> GetWorkspaceLimitAsync(string workspaceId)
    {
        var wsSlug = Slug.FromName(workspaceId);
        return await _dbContext.AiCostLimits
            .FirstOrDefaultAsync(l => l.WorkspaceId == wsSlug && l.ProjectId == null && l.IsActive);
    }

    public async Task<AiCostLimit?> GetProjectLimitAsync(string projectId)
    {
        var projSlug = Slug.FromName(projectId);
        return await _dbContext.AiCostLimits
            .FirstOrDefaultAsync(l => l.ProjectId == projSlug && l.IsActive);
    }

    public async Task<IReadOnlyList<AiCostLimit>> GetWorkspaceLimitsAsync(string workspaceId)
    {
        var wsSlug = Slug.FromName(workspaceId);
        return await _dbContext.AiCostLimits
            .Where(l => l.WorkspaceId == wsSlug && l.ProjectId == null)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AiCostLimit>> GetProjectLimitsAsync(string projectId)
    {
        var projSlug = Slug.FromName(projectId);
        return await _dbContext.AiCostLimits
            .Where(l => l.ProjectId == projSlug)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task SaveLimitAsync(AiCostLimit limit)
    {
        if (limit.Id == 0)
        {
            _dbContext.AiCostLimits.Add(limit);
        }
        else
        {
            _dbContext.AiCostLimits.Update(limit);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteLimitAsync(int id)
    {
        var limit = await _dbContext.AiCostLimits.FindAsync(id);
        if (limit != null)
        {
            _dbContext.AiCostLimits.Remove(limit);
            await _dbContext.SaveChangesAsync();
        }
    }
}
