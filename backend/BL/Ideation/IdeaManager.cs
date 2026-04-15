using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using IAiManager = Conversey.BL.Ai.IAiManager;
using ModerationDecision = Conversey.BL.Ai.ModerationDecision;
using IdeationResponse = Conversey.BL.Domain.Ideation.Response;

namespace Conversey.BL.Ideation;

public class IdeaManager: IIdeaManager
{
    private readonly IIdeaRepository _repository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAiManager _aiManager;

    public IdeaManager(IIdeaRepository repository, IProjectRepository projectRepository, IAiManager aiManager)
    {
        _repository = repository;
        _projectRepository = projectRepository;
        _aiManager = aiManager;
    }

    public SubmissionResponse SubmitIdea(string ideaContent, Slug projectId, int topicId, Guid youthToken) 
    {
        Project forProject = _projectRepository.ReadProjectByIdWithTopics(projectId) 
                             ?? throw new ProjectNotFoundException(projectId.ToString());
        Topic forTopic = (forProject.Topic ?? Array.Empty<Topic>()).SingleOrDefault(t => t.Id == topicId) 
                         ?? throw new TopicNotFoundException(topicId.ToString());
        Youth author = GetYouthInProject(youthToken, projectId);
        
        ModerationDecision decision = EvaluateIdeaModeration(ideaContent);

        ModerationStatus status = decision.IsAllowed ? ModerationStatus.Approved : ModerationStatus.Pending;

        var idea = new Idea
        {
            Content = ideaContent.Trim(),
            Project = forProject,
            Status = status,
            SubmissionDate = DateTime.UtcNow,
            Topic = forTopic,
            Youth = author
        };
        Validate(idea);
        _repository.CreateIdea(idea);

        return decision.IsAllowed ? new SubmissionResponse.Approved(idea) : new SubmissionResponse.Pending(idea, decision);
    }

    public Idea GetIdeaById(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        Idea foundIdea = _repository.ReadIdeaById(ideaId) ?? throw new IdeaNotFoundException(ideaId);
        if (foundIdea.Project.Id != projectId || foundIdea.Project.Workspace.Id != workspaceId || foundIdea.Topic.Id != topicId)
        {
            throw new IdeaNotFoundException(ideaId);
        }
        
        return foundIdea;
    }

    public Idea GetIdeaByIdWithProjectAndResponses(int ideaId)
    {
        return _repository.ReadIdeaByIdWithProjectAndResponses(ideaId) ?? throw new IdeaNotFoundException(ideaId);
    }

    public IEnumerable<Idea> GetIdeasFromProjectByYouthToken(Slug projectId, Guid youthToken)
    {
        return _repository.ReadIdeasFromProjectByYouthToken(projectId, youthToken);
    }

    public IEnumerable<Idea> GetIdeasByProjectIdAndTopicId(Slug projectId, int topicId)
    {
        return _repository.ReadIdeasFromTopicByProjectSlugAndTopicId(projectId, topicId);
    }

    public Idea ChangeIdea(Slug workspaceId, Slug projectId, int topicId, Idea newIdea)
    {
        Idea foundIdea = _repository.ReadIdeaById(newIdea.Id) ?? throw new IdeaNotFoundException(newIdea.Id);
        if (foundIdea.Project.Id != projectId || foundIdea.Project.Workspace.Id != workspaceId || foundIdea.Topic.Id != topicId)
        {
            throw new IdeaNotFoundException(newIdea.Id);
        }
        
        Validate(newIdea);
        _repository.UpdateIdea(newIdea);
        return _repository.ReadIdeaById(newIdea.Id);
    }

    public ResponseSubmissionResponse AddResponse(string text, int ideaId, string youthToken)
    {
        var idea = _repository.ReadIdeaByIdWithProject(ideaId);
        if (idea == null) throw new IdeaNotFoundException(ideaId);

        Youth author = GetYouthForProject(youthToken, idea.Project.Id);
        ModerationDecision decision = EvaluateIdeaModeration(text);

        var response = new IdeationResponse
        {
            Text = text.Trim(),
            Idea = idea,
            CreatedAt = DateTime.UtcNow,
            Youth = author,
            Status = decision.IsAllowed ? ModerationStatus.Approved : ModerationStatus.Pending
        };
        Validate(response);
        _repository.CreateResponse(response);

        return decision.IsAllowed
            ? new ResponseSubmissionResponse.Approved(response)
            : new ResponseSubmissionResponse.Pending(response, decision);
    }

    public IdeationResponse ChangeResponse(IdeationResponse response)
    {
        Validate(response);
        _repository.UpdateResponse(response);
        return response;
    }

    public IdeationResponse GetResponseByIdWithIdea(int responseId)
    {
        return _repository.ReadResponseByIdWithIdea(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
    }

    public IEnumerable<IdeationResponse> GetResponsesFromIdeaByIdeaId(int ideaId)
    {
        return _repository.ReadResponsesFromIdeaByIdeaId(ideaId);
    }

    public IdeaReaction AddIdeaReaction(string emoji, int ideaId, string youthToken)
    {
        var idea = _repository.ReadIdeaByIdWithProject(ideaId) ?? throw new IdeaNotFoundException(ideaId);
        Youth author = GetYouthForProject(youthToken, idea.Project.Id);
        string normalizedEmoji = NormalizeEmoji(emoji);

        var existingReaction = _repository.ReadIdeaReaction(ideaId, author.Id, normalizedEmoji);
        if (existingReaction != null)
        {
            return existingReaction;
        }

        var reaction = new IdeaReaction
        {
            Idea = idea,
            Emoji = normalizedEmoji,
            CreatedAt = DateTime.UtcNow,
            Youth = author
        };
        Validate(reaction);
        _repository.CreateIdeaReaction(reaction);
        return reaction;
    }


    public IEnumerable<IdeaReaction> GetIdeaReactionsByIdeaId(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        Idea foundIdea = _repository.ReadIdeaById(ideaId) ?? throw new IdeaNotFoundException(ideaId);
        if (foundIdea.Project.Id != projectId || foundIdea.Project.Workspace.Id != workspaceId || foundIdea.Topic.Id != topicId)
        {
            throw new IdeaNotFoundException(ideaId);
        }
        
        return _repository.ReadIdeaReactionsFromIdeaByIdeaId(ideaId);
    }

    public void RemoveIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, int reactionId)
    {
        _ = GetIdeaById(workspaceId, projectId, topicId, ideaId);
        var reaction = _repository.ReadIdeaReactionsFromIdeaByIdeaId(ideaId)
            .SingleOrDefault(r => r.Id == reactionId);
        if (reaction == null || reaction.Youth?.Id != youthId)
        {
            throw new IdeaReactionNotFoundException(reactionId);
        }
        
        if (!_repository.DeleteIdeaReaction(reactionId))
        {
            throw new IdeaReactionNotFoundException(reactionId);
        }
    }

    public ResponseReaction AddResponseReaction(string emoji, int responseId, string youthToken)
    {
        var response = _repository.ReadResponseByIdWithIdea(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
        if (response.Idea?.Project == null) throw new ValidationException("Response idea project was not loaded.");
        Youth author = GetYouthForProject(youthToken, response.Idea.Project.Id);
        string normalizedEmoji = NormalizeEmoji(emoji);

        var existingReaction = _repository.ReadResponseReaction(responseId, author.Id, normalizedEmoji);
        if (existingReaction != null)
        {
            return existingReaction;
        }

        var reaction = new ResponseReaction
        {
            Response = response,
            Emoji = normalizedEmoji,
            CreatedAt = DateTime.UtcNow,
            Youth = author
        };
        Validate(reaction);
        _repository.CreateResponseReaction(reaction);
        return reaction;
    }

    public IEnumerable<ResponseReaction> GetResponseReactionsFromResponseByResponseId(int responseId)
    {
        _ = _repository.ReadResponseById(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
        return _repository.ReadResponseReactionsFromResponseByResponseId(responseId);
    }

    public void RemoveResponseReaction(int responseId, string youthToken, string emoji)
    {
        var youthId = ParseYouthToken(youthToken);
        string normalizedEmoji = NormalizeEmoji(emoji);
        if (!_repository.DeleteResponseReaction(responseId, youthId, normalizedEmoji))
        {
            throw new ResponseReactionNotFoundException(responseId, youthToken, normalizedEmoji);
        }
    }

    private Youth GetYouthForProject(string youthToken, Slug projectId)
    {
        var youthId = ParseYouthToken(youthToken);
        return GetYouthInProject(youthId, projectId);
    }

    private Youth GetYouthInProject(Guid youthId, Slug projectId)
    {
        Youth youth = _projectRepository.ReadYouthByTokenWithProject(youthId)
                      ?? throw new YouthNotFoundException(youthId);

        if (youth.Project?.Id != projectId)
        {
            throw new ValidationException("Youth does not exist for this project.");
        }

        return youth;
    }

    private static Guid ParseYouthToken(string youthToken)
    {
        if (!Guid.TryParse(youthToken?.Trim(), out var youthId))
        {
            throw new ValidationException("YouthToken must be a valid GUID.");
        }

        return youthId;
    }

    private static string NormalizeEmoji(string emoji)
    {
        return string.IsNullOrWhiteSpace(emoji) ? string.Empty : emoji.Trim();
    }

    private ModerationDecision EvaluateIdeaModeration(string content)
    {
        Console.WriteLine($"[IdeaManager] Sending content to moderation: \"{content}\"");
        ModerationDecision fallbackDecision = new ModerationDecision { IsAllowed = true };
        
        try
        {
            var decision = _aiManager.ModerateContent(content).Result;

            if (decision.IsAllowed)
            {
                return decision;
            }

            try
            {
                decision.Suggestion = _aiManager.GenerateAiAlternative(content, decision).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IdeaManager] Failed to generate AI alternative: {ex.Message}");
                decision.Suggestion = "Please rephrase your idea in a respectful way.";
            }

            return decision;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IdeaManager] Moderation check failed, allowing by default: {ex.Message}");
            return fallbackDecision;
        }
    }

    private void Validate(object obj)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(obj);

        if (!Validator.TryValidateObject(obj, context, validationResults, true))
        {
            throw new ValidationException(string.Join("; ", validationResults.Select(r => r.ErrorMessage)));
        }
    }
}
