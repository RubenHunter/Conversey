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

    public async Task<bool> IsIdeaAllowedAsync(string ideaDescription)
    {
        var prompt = $"Beoordeel of het volgende idee geschikt is voor publicatie op een platform voor jongeren. Antwoord alleen met 'ja' of 'nee': {ideaDescription}";
        var response = await _aiManager.GenerateResponseAsync(prompt);
        return response.Trim().Equals("ja", StringComparison.OrdinalIgnoreCase);
    }
    
    public async Task<string> GenerateAISuggestionAsync(string prompt)
    {
        return await _aiManager.GenerateResponseAsync(prompt);
    }

}