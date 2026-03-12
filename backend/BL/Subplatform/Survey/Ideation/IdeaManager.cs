using Conversey.BL.Ai;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.DAL.Subplatform.Survey.Ideas;

namespace Conversey.BL.Subplatform.Survey.Ideation;

public class IdeaManager: IIdeaManager
{
    private readonly IIdeaRepository _ideaRepository;
    private readonly IAiManager _aiManager;

    public IdeaManager(IIdeaRepository ideaRepository, IAiManager aiManager)
    {
        _ideaRepository = ideaRepository;
        _aiManager = aiManager;
    }

    public async Task<string> ReviewIdeaAsync(string contentText)
    {
        if (string.IsNullOrWhiteSpace(contentText))
            throw new ArgumentException("Idea text mag niet leeg zijn.", nameof(contentText));

        var moderationDecision = await IsIdeaAllowedAsync(contentText);

        if (moderationDecision.IsAllowed)
        {
            return $"IsAllowed:True";
        }
        
        var alternativeText = await GenerateAIAlternativeAsync(contentText);
        
        return $"IsAllowed:False,AlternativeText:{alternativeText}";
    }
    
    public async Task<ModerationDecision> IsIdeaAllowedAsync(string ideaDescription)
    {
        var decision = await _aiManager.ModerateContentAsync(ideaDescription);
        return decision;
    }
    
    public async Task<string> GenerateAIAlternativeAsync(string prompt)
    {
        return await _aiManager.GenerateAiAlternativeAsync(prompt);
    }
}