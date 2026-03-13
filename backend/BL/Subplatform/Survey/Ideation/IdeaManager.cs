using Conversey.BL.Ai;
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

    public string  SubmitIdea(string contentText, bool forceSubmit = false)
    {
        Idea idea = new Idea
        {
            
        };
        
        if (string.IsNullOrWhiteSpace(contentText))
            throw new InvalidSubmitionException();

        ModerationDecision moderationDecision = IsIdeaAllowed(contentText);

        if (moderationDecision.IsAllowed)
        {
            return $"IsAllowed:True";
        }
        
        var alternativeText = GenerateAiAlternative(contentText);
        
        return $"IsAllowed:False,AlternativeText:{alternativeText}";
    }
    
    public ModerationDecision IsIdeaAllowed(string ideaDescription)
    {
        return _aiManager.ModerateContent(ideaDescription);
    }
    
    public string GenerateAiAlternative(string prompt)
    {
        return _aiManager.GenerateAiAlternative(prompt);
    }
}