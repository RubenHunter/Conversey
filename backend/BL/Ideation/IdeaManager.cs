using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Ai;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using Response = Conversey.BL.Domain.Ideation.Response;

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

    public Idea GetIdeaByIdWithProject(int ideaId)
    {
        return _repository.ReadIdeaByIdWithProject(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
    }

    public Idea GetIdeaByIdWithResponses(int ideaId)
    {
        return _repository.ReadIdeaByIdWithResponses(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
    }

    public Idea GetIdeaByIdWithProjectAndResponses(int ideaId)
    {
        return _repository.ReadIdeaByIdWithProjectAndResponses(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
    }

    public IEnumerable<Idea> GetIdeasFromProjectByYouthToken(int projectId, string youthToken)
    {
        return _repository.ReadIdeasFromProjectByYouthToken(projectId, youthToken);
    }

    public IEnumerable<Idea> GetIdeasByProjectIdAndTopicId(Slug projectId, int topicId)
    {
        return _repository.ReadIdeasByProjectIdAndTopicId(projectId, topicId);
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
        if (idea == null) throw new IdeaNotFoundException(ideaId.ToString());

        Youth author = GetYouthForProject(youthToken, idea.Project.Id);
        ModerationDecision decision = EvaluateIdeaModeration(text);

        var response = new IdeaResponse
        {
            Text = text.Trim(),
            Idea = idea,
            CreatedAt = DateTime.UtcNow,
            Youth = author,
            //Status = allowed ? IdeaStatus.Approved : IdeaStatus.Pending
            Status = decision.IsAllowed ? IdeaStatus.Approved : IdeaStatus.Pending
        };
        Validate(response);
        _repository.CreateResponse(response);

        return decision.IsAllowed
            ? new ResponseSubmissionResponse.Approved(response)
            : new ResponseSubmissionResponse.Pending(response, decision);
    }

    Response IIdeaManager.GetResponseById(int responseId)
    {
        throw new NotImplementedException();
    }

    Response IIdeaManager.GetResponseByIdWithIdea(int responseId)
    {
        throw new NotImplementedException();
    }

    IEnumerable<Response> IIdeaManager.GetResponsesFromIdeaByIdeaId(int ideaId)
    {
        throw new NotImplementedException();
    }

    public Response ChangeResponse(Response response)
    {
        throw new NotImplementedException();
    }

    public IdeaResponse GetResponseById(int responseId)
    {
        return _repository.ReadResponseById(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
    }

    public IdeaResponse GetResponseByIdWithIdea(int responseId)
    {
        return _repository.ReadResponseByIdWithIdea(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
    }

    public IEnumerable<IdeaResponse> GetResponsesFromIdeaByIdeaId(int ideaId)
    {
        return _repository.ReadResponsesFromIdeaByIdeaId(ideaId);
    }

    public IdeaReaction AddIdeaReaction(string emoji, int ideaId, string youthToken)
    {
        var idea = _repository.ReadIdeaByIdWithProject(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
        Youth author = GetYouthForProject(youthToken, idea.Project.Id);
        string normalizedEmoji = NormalizeEmoji(emoji);

        var existingReaction = _repository.ReadIdeaReaction(ideaId, author.Token, normalizedEmoji);
        if (existingReaction != null)
        {
            return existingReaction;
        }

        var reaction = new IdeaReaction
        {
            IdeaId = idea.Id,
            Idea = idea,
            Emoji = normalizedEmoji,
            CreatedAt = DateTime.UtcNow,
            YouthToken = author.Token,
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
        
        return _repository.ReadIdeaReactionsByIdeaId(ideaId);
    }

    public void RemoveIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, int reactionId)
    {
        Idea foundIdea = _repository.ReadIdeaByIdWithTopicAndYouthAndReactionsAndProjectWithWorkspace(ideaId) ?? throw new IdeaNotFoundException(ideaId);
        if (foundIdea.Project.Id != projectId || 
            foundIdea.Project.Workspace.Id != workspaceId || 
            foundIdea.Topic.Id != topicId || 
            foundIdea.Youth.Id != youthId)
        {
            throw new IdeaNotFoundException(ideaId);
        }
        if (foundIdea.Reactions.SingleOrDefault(r => r.Id == reactionId) != null)
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

        var existingReaction = _repository.ReadResponseReaction(responseId, author.Token, normalizedEmoji);
        if (existingReaction != null)
        {
            return existingReaction;
        }

        var reaction = new ResponseReaction
        {
            ResponseId = response.Id,
            Response = response,
            Emoji = normalizedEmoji,
            CreatedAt = DateTime.UtcNow,
            YouthToken = author.Token,
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
        string normalizedEmoji = NormalizeEmoji(emoji);
        if (!_repository.DeleteResponseReaction(responseId, youthToken, normalizedEmoji))
        {
            throw new ResponseReactionNotFoundException(responseId, youthToken, normalizedEmoji);
        }
    }

    private Youth GetYouthInProject(Guid youthId, Slug projectId)
    {
        Youth youth = _projectRepository.ReadYouthByTokenWithProject(youthId)
                      ?? throw new YouthNotFoundException(youthId);

        if (youth.Project?.Id != projectId)
        {
            throw new ValidationException("Youth does exist for this project.");
        }

        return youth;
    }

    private static string NormalizeEmoji(string emoji)
    {
        return string.IsNullOrWhiteSpace(emoji) ? string.Empty : emoji.Trim();
    }

    private ModerationDecision EvaluateIdeaModeration(string content)
    {
        Console.WriteLine($"[IdeaManager] 🔍 Sending content to Mistral AI for moderation: \"{content}\"");
        ModerationDecision decision =  new ModerationDecision();
        
        try
        {
            var decision = _aiManager.ModerateContent(content).Result;

            if (decision.IsAllowed)
            {
                Console.WriteLine("[IdeaManager] ✅ Mistral AI: content is ALLOWED");
                return decision;
            }

            Console.WriteLine("[IdeaManager] ⚠️ Mistral AI: content is FLAGGED — generating alternative...");

            try
            {
                suggestion = _aiManager.GenerateAiAlternative(content, decision).Result;
                Console.WriteLine($"[IdeaManager] 💬 AI alternative: \"{suggestion}\"");
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
            Console.WriteLine($"[IdeaManager] ❌ Moderation check failed, allowing by default: {ex.Message}");
            return decision;
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
