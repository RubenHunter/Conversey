namespace Conversey.BL.Subplatform.Survey.Ideation;

public interface IIdeaManager
{
    Task<bool> IsIdeaAllowedAsync(string ideaDescription);
    Task<string> GenerateAISuggestionAsync(string prompt);
}