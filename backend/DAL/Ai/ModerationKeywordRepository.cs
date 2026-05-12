using Conversey.BL.Domain.Ai;
using Microsoft.EntityFrameworkCore;

namespace Conversey.DAL.Subplatform.Ai;

public class ModerationKeywordRepository : IModerationKeywordRepository
{
    private readonly ConverseyDbContext _dbContext;

    public ModerationKeywordRepository(ConverseyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<ModerationKeyword> GetAll()
    {
        return _dbContext.ModerationKeywords.OrderBy(k => k.Keyword).ToList();
    }

    public IReadOnlyList<ModerationKeyword> GetForWorkspace(string workspaceId)
    {
        return _dbContext.ModerationKeywords
            .Where(k => k.WorkspaceId == null || k.WorkspaceId == workspaceId)
            .OrderBy(k => k.Keyword)
            .ToList();
    }

    public IReadOnlySet<string> GetKeywordSet()
    {
        var keywords = _dbContext.ModerationKeywords
            .Select(k => k.Keyword)
            .ToList();
        return new HashSet<string>(keywords, StringComparer.OrdinalIgnoreCase);
    }

    public void Save(ModerationKeyword keyword)
    {
        keyword.Keyword = (keyword.Keyword ?? string.Empty).Trim().ToLowerInvariant();

        var existing = _dbContext.ModerationKeywords.FirstOrDefault(k => k.Id == keyword.Id);
        if (existing != null)
        {
            existing.Keyword = keyword.Keyword;
        }
        else
        {
            keyword.CreatedAt = DateTime.UtcNow;
            _dbContext.ModerationKeywords.Add(keyword);
        }

        _dbContext.SaveChanges();
    }

    public void Delete(int id)
    {
        var keyword = _dbContext.ModerationKeywords.Find(id);
        if (keyword != null)
        {
            _dbContext.ModerationKeywords.Remove(keyword);
            _dbContext.SaveChanges();
        }
    }
}
