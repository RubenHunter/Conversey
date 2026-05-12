using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IModerationKeywordRepository
{
    IReadOnlyList<ModerationKeyword> GetAll();
    IReadOnlyList<ModerationKeyword> GetForWorkspace(string workspaceId);
    void Save(ModerationKeyword keyword);
    void Delete(int id);
    IReadOnlySet<string> GetKeywordSet();
}
