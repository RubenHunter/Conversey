using Conversey.BL.Ai;
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

    public async Task<ModerationDecision> IsIdeaAllowedAsync(string ideaDescription)
    {
        var decision = await _aiManager.ModerateContentAsync(ideaDescription);
        
        return decision;
    }
    
    public async Task<string> GenerateAISuggestionAsync(string prompt)
    {
        return await _aiManager.GenerateResponseAsync(prompt);
    }

}