using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Ideation;
using IAiManager = Conversey.BL.Ai.IAiManager;
using ModerationDecision = Conversey.BL.Ai.ModerationDecision;

namespace Conversey.BL.Ideation;

public class IdeaManager: IIdeaManager
{
    private readonly IIdeaRepository _repository;
    private readonly IProjectManager _projectManager;
    private readonly IAiManager _aiManager;

    public IdeaManager(IIdeaRepository repository, IAiManager aiManager, IProjectManager projectManager)
    {
        _repository = repository;
        _aiManager = aiManager;
        _projectManager = projectManager;
    }

    public SubmissionResponse SubmitIdea(Slug workspaceId, Slug projectId, int topicId, Guid youthId, string ideaContent)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Youth author = _projectManager.GetYouth(project, youthId);
        
        ModerationDecision decision = EvaluateIdeaModeration(ideaContent);

        ModerationStatus status = decision.IsAllowed ? ModerationStatus.Approved : ModerationStatus.Pending;

        var idea = new Idea
        {
            Content = ideaContent.Trim(),
            Project = project,
            Status = status,
            SubmissionDate = DateTime.UtcNow,
            Topic = topic,
            Youth = author
        };
        Validate(idea);
        _repository.CreateIdea(idea);

        return decision.IsAllowed ? new SubmissionResponse.Approved(idea) : new SubmissionResponse.Pending(idea, decision);
    }

    public Idea GetIdea(Topic topic, int ideaId)
    {
        Idea foundIdea = _repository.ReadIdeaById(ideaId);
        if (!topic.Ideas.Contains(foundIdea))
        {
           throw new IdeaNotFoundException(ideaId);
        }

        return foundIdea;
    }

    public Idea GetIdeaById(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        return GetIdea(topic, ideaId);
    }

    public Idea GetIdeaByIdWithProjectAndResponses(Slug workspaceId, Slug projectId, int topicId, int ideaId)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Idea idea = _repository.ReadIdeaByIdWithProjectAndResponses(ideaId);
        if (idea == null || idea.Topic.Id != topic.Id)
        {
            throw new IdeaNotFoundException(ideaId);
        }

        return idea;
    }

    public IEnumerable<Idea> GetIdeasFromProjectByYouthId(Slug workspaceId, Slug projectId, Guid youthId)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        _projectManager.GetYouth(project, youthId);
        return _repository.ReadIdeasByYouthId(youthId);
    }

    public IEnumerable<Idea> GetIdeasByProjectIdAndTopicId(Slug workspaceId, Slug projectId, int topicId)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        _projectManager.GetTopic(project, topicId);
        return _repository.ReadIdeasByTopicId(topicId);
    }

    public Idea ChangeIdea(Slug workspaceId, Slug projectId, int topicId, int ideaId, ModerationStatus newStatus, string newContent)
    {
        Idea foundIdea = _repository.ReadIdeaById(ideaId) ?? throw new IdeaNotFoundException(ideaId);
        if (foundIdea.Project.Id != projectId || foundIdea.Project.Workspace.Id != workspaceId || foundIdea.Topic.Id != topicId)
        {
            throw new IdeaNotFoundException(ideaId);
        }

        foundIdea.Status = newStatus;
        foundIdea.Content = newContent;
        
        Validate(foundIdea);
        _repository.UpdateIdea(foundIdea);
        return foundIdea;
    }

    public IdeaResponse GetResponse(Idea idea, int responseId)
    {
        IdeaResponse ideaResponse = _repository.ReadResponseById(responseId);
        if (ideaResponse == null || ideaResponse.Idea.Id != idea.Id)
        {
            throw new ResponseNotFoundException(idea.Id);
        }

        return ideaResponse;
    }

    public ResponseSubmissionResponse AddResponse(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, string responseText)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Youth author = _projectManager.GetYouth(project, youthId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Idea idea = GetIdea(topic, ideaId);

        responseText = responseText.Trim();
        ModerationDecision decision = EvaluateIdeaModeration(responseText);

        var response = new IdeaResponse
        {
            Text = responseText,
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

    public IdeaResponse ChangeResponse(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, int responseId, ModerationStatus newStatus, string responseText)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        _projectManager.GetYouth(project, youthId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Idea idea = GetIdea(topic, ideaId);
        IdeaResponse response = GetResponse(idea, responseId);
        
        response.Status = newStatus;
        response.Text = responseText;
        
        Validate(response);
        _repository.UpdateResponse(response);
        return response;
    }

    public IEnumerable<IdeaResponse> GetApprovedResponsesByYouth(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        _projectManager.GetYouth(project, youthId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        GetIdea(topic, ideaId);
        return _repository.ReadApprovedResponsesByYouthIdAndIdeaId(ideaId, youthId);
    }

    public IdeaReaction AddIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, string emoji)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Idea idea = GetIdea(topic, ideaId);
        Youth author = _projectManager.GetYouth(project, youthId);

        string normalizedEmoji = NormalizeEmoji(emoji);
        
        IdeaReaction existingReaction = _repository.ReadIdeaReactionByEmoji(normalizedEmoji);
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
        
        return _repository.ReadIdeaReactionsByIdeaId(ideaId);
    }

    public void RemoveIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, int reactionId)
    {
        _ = GetIdeaById(workspaceId, projectId, topicId, ideaId);
        var reaction = _repository.ReadIdeaReactionsByIdeaId(ideaId)
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

    public ResponseReaction AddResponseReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, Guid youthId, string emoji)
    {
        IdeaResponse response = _repository.ReadResponseByIdWithIdea(responseId) ?? throw new ResponseNotFoundException(responseId);
        if (response.Idea?.Project == null) throw new ValidationException("Response idea project was not loaded.");
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Youth author = _projectManager.GetYouth(project, youthId);
        string normalizedEmoji = NormalizeEmoji(emoji);

        ResponseReaction existingReaction = _repository.ReadResponseReactionByEmoji(normalizedEmoji);
        if (existingReaction != null)
        {
            return existingReaction;
        }

        var reaction = new ResponseReaction
        {
            IdeaResponse = response,
            Emoji = normalizedEmoji,
            CreatedAt = DateTime.UtcNow,
            Youth = author
        };
        Validate(reaction);
        _repository.CreateResponseReaction(reaction);
        return reaction;
    }

    public IEnumerable<ResponseReaction> GetResponseReactionsByResponseId(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Idea idea = GetIdea(topic, ideaId);
        GetResponse(idea, responseId);
        return _repository.ReadResponseReactionsByResponseId(responseId);
    }

    public ResponseReaction GetResponseReaction(IdeaResponse response, int reactionId)
    {
        ResponseReaction reaction = _repository.ReadResponseReactionByReactionId(reactionId);
        if (reaction == null || !response.Reactions.Contains(reaction))
        {
            throw new ResponseReactionNotFoundException(reactionId);
        }
        return reaction;
    }

    public void RemoveResponseReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, int responseId, Guid youthId, int reactionId)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        _projectManager.GetYouth(project, youthId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Idea idea = GetIdea(topic, ideaId);
        IdeaResponse response = GetResponse(idea, responseId);
        GetResponseReaction(response, reactionId);
        if (!_repository.DeleteResponseReaction(reactionId))
        {
            throw new ResponseReactionNotFoundException(reactionId);
        }
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
