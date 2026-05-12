using Conversey.BL.Domain.Ai;
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
        DateTime startTime, TimeSpan duration, string providerName = "", string promptName = "")
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
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.AiAuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<AiAuditLog>> GetAiCostsAsync()
    {
        return await _dbContext.AiAuditLogs.OrderByDescending(log => log.CreatedAt).ToListAsync();
    }
}