using System.ComponentModel.DataAnnotations;
using Conversey.BL.Ai;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Administration;
using Conversey.DAL.Ideation;
using IdeaResponse = Conversey.BL.Domain.Ideation.Response;

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

    public SubmissionResponse SubmitIdea(string content, Slug projectSlug, int topicId, Guid youthToken) 
    {
        Project forProject = _projectRepository.ReadProjectByIdWithTopics(projectSlug) 
                             ?? throw new ProjectNotFoundException(projectSlug.Text);
        Topic forTopic = (forProject.Topic ?? Array.Empty<Topic>()).SingleOrDefault(t => t.Id == topicId) 
                         ?? throw new TopicNotFoundException(topicId.ToString());
        Youth author = GetYouthForProject(youthToken, projectSlug);
        
        ModerationDecision decision = EvaluateIdeaModeration(content);

        ModerationStatus status = decision.IsAllowed ? ModerationStatus.Approved : ModerationStatus.Pending;

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

    public IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectId(Slug projectSlug)
    {
        return _repository.ReadIdeasFromProjectByProjectId(projectSlug);
    }

    public IReadOnlyCollection<Idea> GetIdeasFromProjectByProjectIdWithResponses(Slug projectSlug)
    {
        return _repository.ReadIdeasFromProjectByProjectIdWithResponses(projectSlug);
    }

    public IReadOnlyCollection<Idea> GetIdeasFromProjectByYouthToken(Slug projectSlug, Guid youthToken)
    {
        return _repository.ReadIdeasFromProjectByYouthToken(projectSlug, youthToken);
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

    public ResponseSubmissionResponse AddResponse(string text, int ideaId, Guid youthToken)
    {
        var idea = _repository.ReadIdeaByIdWithProject(ideaId);
        if (idea == null) throw new IdeaNotFoundException(ideaId.ToString());

        Youth author = GetYouthForProject(youthToken, idea.Project.Slug);
        ModerationDecision decision = EvaluateIdeaModeration(text);

        var response = new IdeaResponse
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

    public IdeaReaction AddIdeaReaction(string emoji, int ideaId, Guid youthToken)
    {
        var idea = _repository.ReadIdeaByIdWithProject(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
        Youth author = GetYouthForProject(youthToken, idea.Project.Slug);
        string normalizedEmoji = NormalizeEmoji(emoji);

        var existingReaction = _repository.ReadIdeaReaction(ideaId, author.Token, normalizedEmoji);
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

    public IReadOnlyCollection<IdeaReaction> GetIdeaReactionsFromIdeaByIdeaId(int ideaId)
    {
        _ = _repository.ReadIdeaById(ideaId) ?? throw new IdeaNotFoundException(ideaId.ToString());
        return _repository.ReadIdeaReactionsFromIdeaByIdeaId(ideaId);
    }

    public void RemoveIdeaReaction(int ideaId, Guid youthToken, string emoji)
    {
        string normalizedEmoji = NormalizeEmoji(emoji);
        if (!_repository.DeleteIdeaReaction(ideaId, youthToken, normalizedEmoji))
        {
            throw new IdeaReactionNotFoundException(ideaId, youthToken, normalizedEmoji);
        }
    }

    public ResponseReaction AddResponseReaction(string emoji, int responseId, Guid youthToken)
    {
        var response = _repository.ReadResponseByIdWithIdea(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
        if (response.Idea?.Project == null) throw new ValidationException("Response idea project was not loaded.");
        Youth author = GetYouthForProject(youthToken, response.Idea.Project.Slug);
        string normalizedEmoji = NormalizeEmoji(emoji);

        var existingReaction = _repository.ReadResponseReaction(responseId, author.Token, normalizedEmoji);
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

    public IReadOnlyCollection<ResponseReaction> GetResponseReactionsFromResponseByResponseId(int responseId)
    {
        _ = _repository.ReadResponseById(responseId) ?? throw new ResponseNotFoundException(responseId.ToString());
        return _repository.ReadResponseReactionsFromResponseByResponseId(responseId);
    }

    public void RemoveResponseReaction(int responseId, Guid youthToken, string emoji)
    {
        string normalizedEmoji = NormalizeEmoji(emoji);
        if (!_repository.DeleteResponseReaction(responseId, youthToken, normalizedEmoji))
        {
            throw new ResponseReactionNotFoundException(responseId, youthToken, normalizedEmoji);
        }
    }

    private Youth GetYouthForProject(Guid youthToken, Slug projectSlug)
    {
        Youth youth = _projectRepository.ReadYouthByTokenWithProject(youthToken)
                      ?? throw new YouthNotFoundException(youthToken.ToString());

        if (youth.Project?.Slug != projectSlug)
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
        Console.WriteLine($"[IdeaManager] 🔍 Sending content to AI moderation: \"{content}\"");
        
        try
        {
            var decision = _aiManager.ModerateContent(content).Result;

            if (decision.IsAllowed)
            {
                Console.WriteLine("[IdeaManager] ✅ AI moderation: content is ALLOWED");
                return decision;
            }

            Console.WriteLine("[IdeaManager] ⚠️ AI moderation: content is FLAGGED — generating alternative...");

            try
            {
                var suggestion = _aiManager.GenerateAiAlternative(content, decision).Result;
                Console.WriteLine($"[IdeaManager] 💬 AI alternative: \"{suggestion}\"");
                decision.Suggestion = suggestion;
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
            Console.WriteLine($"[IdeaManager] ❌ Moderation check failed, marking content as pending: {ex.Message}");
            return new ModerationDecision
            {
                IsAllowed = false,
                Suggestion = "We could not complete moderation right now. Please rephrase your message in a respectful way."
            };
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
