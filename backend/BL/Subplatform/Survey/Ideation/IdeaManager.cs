using System.ComponentModel.DataAnnotations;
using Conversey.BL.Ai;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Subplatform.Survey;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;
using Conversey.DAL.Subplatform.Survey;
using Conversey.DAL.Subplatform.Survey.Ideas;
using IdeaResponse = Conversey.BL.Domain.Subplatform.Survey.Ideation.Response;

namespace Conversey.BL.Subplatform.Survey.Ideation;

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

    public SubmissionResponse SubmitIdea(string content, int projectId, int topicId, string youthToken) 
    {
        Project forProject = _projectRepository.ReadProjectByIdWithTopics(projectId) 
                             ?? throw new ProjectNotFoundException(projectId.ToString());
        Topic forTopic = (forProject.Topic ?? Array.Empty<Topic>()).SingleOrDefault(t => t.Id == topicId) 
                         ?? throw new TopicNotFoundException(topicId.ToString());
        Youth author = GetYouthForProject(youthToken, projectId);
        
        ModerationDecision decision = EvaluateIdeaModeration(content);

        IdeaStatus status = decision.IsAllowed ? IdeaStatus.Approved : IdeaStatus.Pending;

        var idea = new Idea
        {
            Content = content.Trim(),
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

    public Idea GetIdeaById(int ideaId)
    {
        return _repository.ReadIdeaById(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
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

    public IReadOnlyCollection<Idea> GetAllIdeas()
    {
        return _repository.ReadAllIdeas();
    }

    public IReadOnlyCollection<Idea> GetAllIdeasWithProject()
    {
        return _repository.ReadAllIdeasWithProject();
    }

    public IReadOnlyCollection<Idea> GetAllIdeasWithResponses()
    {
        return _repository.ReadAllIdeasWithResponses();
    }

    public IReadOnlyCollection<Idea> GetAllIdeasWithProjectAndResponses()
    {
        return _repository.ReadAllIdeasWithProjectAndResponses();
    }

    public IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectId(int projectId)
    {
        return _repository.ReadIdeasFromProjectByProjectId(projectId);
    }

    public IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectIdWithResponses(int projectId)
    {
        return _repository.ReadIdeasFromProjectByProjectIdWithResponses(projectId);
    }

    public IReadOnlyCollection<Idea> GetIdeasFromProjectByYouthToken(int projectId, string youthToken)
    {
        return _repository.ReadIdeasFromProjectByYouthToken(projectId, youthToken);
    }

    public IReadOnlyCollection<Idea> GetIdeasFromTopicByProjectSlugAndTopicId(Slug projectSlug, int topicId)
    {
        return _repository.ReadIdeasFromTopicByProjectSlugAndTopicId(projectSlug, topicId);
    }

    public Idea ChangeIdea(Idea idea)
    {
        Validate(idea);
        _repository.UpdateIdea(idea);
        return idea;
    }

    public void RemoveIdea(int ideaId)
    {
        if (!_repository.DeleteIdea(ideaId))
        {
            throw new IdeaNotFoundException(ideaId.ToString());
        }
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

    public IdeaResponse GetResponseById(int responseId)
    {
        return _repository.ReadResponseById(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
    }

    public IdeaResponse GetResponseByIdWithIdea(int responseId)
    {
        return _repository.ReadResponseByIdWithIdea(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
    }

    public IReadOnlyCollection<IdeaResponse> GetResponsesFromIdeaByIdeaId(int ideaId)
    {
        return _repository.ReadResponsesFromIdeaByIdeaId(ideaId);
    }

    public IReadOnlyCollection<IdeaResponse> GetResponsesFromIdeaByIdeaIdWithIdea(int ideaId)
    {
        return _repository.ReadResponsesFromIdeaByIdeaIdWithIdea(ideaId);
    }

    public IdeaResponse ChangeResponse(IdeaResponse response)
    {
        Validate(response);
        _repository.UpdateResponse(response);
        return response;
    }

    public void RemoveResponse(int responseId)
    {
        if (!_repository.DeleteResponse(responseId))
        {
            throw new ResponseNotFoundException(responseId.ToString());
        }
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

    public IReadOnlyCollection<IdeaReaction> GetIdeaReactionsFromIdeaByIdeaId(int ideaId)
    {
        _ = _repository.ReadIdeaById(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
        return _repository.ReadIdeaReactionsFromIdeaByIdeaId(ideaId);
    }

    public void RemoveIdeaReaction(int ideaId, string youthToken, string emoji)
    {
        string normalizedEmoji = NormalizeEmoji(emoji);
        if (!_repository.DeleteIdeaReaction(ideaId, youthToken, normalizedEmoji))
        {
            throw new IdeaReactionNotFoundException(ideaId, youthToken, normalizedEmoji);
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

    public IReadOnlyCollection<ResponseReaction> GetResponseReactionsFromResponseByResponseId(int responseId)
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

    private Youth GetYouthForProject(string youthToken, int projectId)
    {
        Youth youth = _projectRepository.ReadYouthByTokenWithProject(youthToken)
                      ?? throw new YouthNotFoundException(youthToken);

        if (youth.Project?.Id != projectId)
        {
            throw new ValidationException("Youth token does not belong to this project.");
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
