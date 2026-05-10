using Conversey.BL.Domain.Ai;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Ai;

public class PromptRepository : IPromptRepository
{
    private readonly ConverseyDbContext _dbContext;

    public PromptRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AiPrompt> GetPromptAsync(string name)
    {
        return _dbContext.AiPrompts.FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<IReadOnlyList<AiPrompt>> GetAllPromptsAsync()
    {
        return await _dbContext.AiPrompts.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task SavePromptAsync(AiPrompt prompt)
    {
        var existing = await _dbContext.AiPrompts.FirstOrDefaultAsync(p => p.Id == prompt.Id);
        if (existing != null)
        {
            existing.Name = prompt.Name;
            existing.SystemPrompt = prompt.SystemPrompt;
            existing.UserPromptTemplate = prompt.UserPromptTemplate;
            existing.Description = prompt.Description;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            prompt.CreatedAt = DateTime.UtcNow;
            prompt.UpdatedAt = DateTime.UtcNow;
            _dbContext.AiPrompts.Add(prompt);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeletePromptAsync(int id)
    {
        var prompt = await _dbContext.AiPrompts.FindAsync(id);
        if (prompt != null)
        {
            _dbContext.AiPrompts.Remove(prompt);
            await _dbContext.SaveChangesAsync();
        }
    }
}
