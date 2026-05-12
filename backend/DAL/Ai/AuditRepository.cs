using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Ai;

public class AuditRepository : IAuditRepository
{
    private readonly ConverseyDbContext _dbContext;

    public AuditRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAiCallAsync(string modelName, string modelType, int inputTokens, int outputTokens, decimal cost,
        DateTime startTime, TimeSpan duration, string providerName = "", string promptName = "", string? workspaceId = null, string? projectId = null)
    {
        var auditLog = new AiAuditLog
        {
            ModelName = modelName,
            ModelType = modelType,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            Cost = cost,
            StartTime = startTime,
            Duration = duration,
            ProviderName = providerName,
            PromptName = promptName,
            CreatedAt = DateTime.UtcNow,
            WorkspaceId = !string.IsNullOrEmpty(workspaceId) ? Slug.FromName(workspaceId) : null,
            ProjectId = !string.IsNullOrEmpty(projectId) ? Slug.FromName(projectId) : null
        };

        _dbContext.AiAuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<AiAuditLog>> GetAiCostsAsync()
    {
        return await _dbContext.AiAuditLogs.OrderByDescending(log => log.CreatedAt).ToListAsync();
    }

    public async Task<IReadOnlyCollection<AiAuditLog>> GetAiCostsFilteredAsync(
        string? workspaceId = null,
        string? projectId = null,
        string? modelName = null,
        string? modelType = null,
        string? providerName = null,
        string? promptName = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = _dbContext.AiAuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(workspaceId))
        {
            var wsSlug = Slug.FromName(workspaceId);
            query = query.Where(l => l.WorkspaceId == wsSlug);
        }

        if (!string.IsNullOrEmpty(projectId))
        {
            var projSlug = Slug.FromName(projectId);
            query = query.Where(l => l.ProjectId == projSlug);
        }

        if (!string.IsNullOrEmpty(modelName))
            query = query.Where(l => l.ModelName == modelName);

        if (!string.IsNullOrEmpty(modelType))
            query = query.Where(l => l.ModelType == modelType);

        if (!string.IsNullOrEmpty(providerName))
            query = query.Where(l => l.ProviderName == providerName);

        if (!string.IsNullOrEmpty(promptName))
            query = query.Where(l => l.PromptName == promptName);

        if (dateFrom.HasValue)
            query = query.Where(l => l.StartTime >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(l => l.StartTime <= dateTo.Value);

        return await query.OrderByDescending(l => l.StartTime).ToListAsync();
    }

    public async Task<decimal> GetTotalCostForWorkspaceAsync(string workspaceId, DateTime periodStart, DateTime periodEnd)
    {
        var wsSlug = Slug.FromName(workspaceId);
        return await _dbContext.AiAuditLogs
            .Where(l => l.WorkspaceId == wsSlug && l.StartTime >= periodStart && l.StartTime <= periodEnd)
            .SumAsync(l => l.Cost);
    }

    public async Task<decimal> GetTotalCostForProjectAsync(string projectId, DateTime periodStart, DateTime periodEnd)
    {
        var projSlug = Slug.FromName(projectId);
        return await _dbContext.AiAuditLogs
            .Where(l => l.ProjectId == projSlug && l.StartTime >= periodStart && l.StartTime <= periodEnd)
            .SumAsync(l => l.Cost);
    }

    public async Task<Dictionary<string, decimal>> GetCostsPerProjectForWorkspaceAsync(string workspaceId, DateTime periodStart, DateTime periodEnd)
    {
        var wsSlug = Slug.FromName(workspaceId);
        var logs = await _dbContext.AiAuditLogs
            .Where(l => l.WorkspaceId == wsSlug && l.ProjectId.HasValue && l.StartTime >= periodStart && l.StartTime <= periodEnd)
            .ToListAsync();

        return logs
            .GroupBy(l => l.ProjectId!.Value.Text)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Cost));
    }
}