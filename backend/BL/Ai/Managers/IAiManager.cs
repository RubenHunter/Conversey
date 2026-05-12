using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ai;

public interface IAiManager
{
    string GenerateAlternative(string content, ModerationDecision decision = null);
    ModerationDecision ModerateContent(string content);
    IdeaNudgeDecision AssessIdeaNudge(IdeaNudgeAssessmentRequest request);
    IEnumerable<int> RankIdeasByRelation(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit);
    IReadOnlyDictionary<int, IReadOnlyList<string>> CategorizeIdeas(
        IReadOnlyList<string> ideas,
        IReadOnlyList<string> existingCategories,
        int maxCategoriesPerIdea);
}
