using Conversey.BL.Domain.Ai;

namespace Conversey.DAL.Subplatform.Ai;

public interface IModerationKeywordRepository
{
    IReadOnlyList<ModerationKeyword> GetAll();
    void Save(ModerationKeyword keyword);
    void Delete(int id);
    IReadOnlySet<string> GetKeywordSet();
}
