using System.ComponentModel.DataAnnotations;
using Conversey.BL.Administration;
using Conversey.BL.Ai;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Administration;
using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Ideation;

namespace Conversey.BL.Ideation;

public class IdeaManager: IIdeaManager
{
    private const int DiscoveryCandidateLimit = 30;
    private const int CategorizationBatchSize = 20;
    private const int MaxCategoriesPerIdea = 3;
    private readonly IIdeaRepository _repository;
    private readonly IProjectManager _projectManager;
    private readonly IAiManager _aiManager;

    public IdeaManager(IIdeaRepository repository, IAiManager aiManager, IProjectManager projectManager)
    {
        _repository = repository;
        _aiManager = aiManager;
        _projectManager = projectManager;
    }

    public async Task<SubmissionResponse> SubmitIdeaAsync(Slug workspaceId, Slug projectId, int topicId, Guid youthId, string ideaContent, bool qualityNudgeBypassed = false)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Youth author;
        try
        {
            author = _projectManager.GetYouth(project, youthId);
        }
        catch (YouthNotFoundException)
        {
            author = _projectManager.AddYouth(youthId, $"{youthId:N}@local.invalid", project.Id);
        }
        
        ModerationDecision decision = await EvaluateIdeaModerationAsync(ideaContent, workspaceId.Text, projectId.Text);

        ModerationStatus status = decision.IsAllowed ? ModerationStatus.Approved : ModerationStatus.Pending;
        if (qualityNudgeBypassed)
        {
            status = ModerationStatus.Pending;
        }

        var idea = new Idea
        {
            Content = ideaContent.Trim(),
            Project = project,
            Status = status,
            QualityNudgeBypassed = qualityNudgeBypassed,
            SubmissionDate = DateTime.UtcNow,
            Topic = topic,
            Youth = author
        };
        Validate(idea);
        _repository.CreateIdea(idea);
        await AssignSemanticCategoriesToIdeaAsync(idea, topicId, workspaceId.Text, projectId.Text);

        return decision.IsAllowed && !qualityNudgeBypassed
            ? new SubmissionResponse.Approved(idea)
            : new SubmissionResponse.Pending(idea, decision);
    }

    public async Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(Slug workspaceId, Slug projectId, int topicId, string ideaContent, IEnumerable<IdeaNudgeTurn> conversation)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        var nudgingStrength = Math.Clamp(project.NudgingStrength, 1, 5);
        var maxRounds = GetMaxNudgingRounds(nudgingStrength);
        var convoList = conversation?.ToList() ?? new List<IdeaNudgeTurn>();
        var roundCount = convoList.Count;

        // Guardrail against endless follow-up loops.
        if (roundCount >= maxRounds)
        {
            return new IdeaNudgeDecision { IsApproved = true };
        }

        try
        {
            var request = new IdeaNudgeAssessmentRequest
            {
                ProjectTitle = project.Name,
                ProjectDescription = project.Description,
                TopicTitle = topic.Name,
                TopicPrompt = topic.Context,
                IdeaText = ideaContent,
                Conversation = convoList,
                NudgingMode = MapStrengthToNudgingMode(nudgingStrength),
            };

            var decision = await _aiManager.AssessIdeaNudgeAsync(request, workspaceId.Text, projectId.Text);
            if (decision == null)
            {
                return new IdeaNudgeDecision { IsApproved = true };
            }

            if (!decision.IsApproved && string.IsNullOrWhiteSpace(decision.Question))
            {
                decision.Question = "Could you make this idea a bit more specific for this topic?";
            }

            return decision;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IdeaManager] Idea nudging failed, allowing by default: {ex.Message}");
            return new IdeaNudgeDecision { IsApproved = true };
        }
    }

    private static int GetMaxNudgingRounds(int nudgingStrength)
    {
        return nudgingStrength switch
        {
            1 => 1,
            2 => 1,
            3 => 2,
            4 => 3,
            5 => 4,
            _ => 2
        };
    }

    private static NudgingMode MapStrengthToNudgingMode(int nudgingStrength)
    {
        return nudgingStrength switch
        {
            1 => NudgingMode.Minimal,
            2 => NudgingMode.Light,
            3 => NudgingMode.Medium,
            4 => NudgingMode.Strong,
            5 => NudgingMode.Deep,
            _ => NudgingMode.Medium
        };
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
        // Validate workspace/project context, but do not fail when a youth has no records yet.
        _projectManager.GetProjectById(workspaceId, projectId);

        return _repository.ReadIdeasByYouthId(youthId)
            .Where(idea => idea.Project?.Id == projectId);
    }

    public IEnumerable<Idea> GetIdeasByProjectIdAndTopicId(Slug workspaceId, Slug projectId, int topicId)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        _projectManager.GetTopic(project, topicId);

        var ideas = _repository.ReadIdeasByTopicId(topicId).ToList();
        EnsureSemanticCategoriesForIdeas(ideas);
        return ideas;
    }

    public async Task<IEnumerable<Idea>> GetIdeaDiscoverySuggestionsAsync(
        Slug workspaceId,
        Slug projectId,
        int topicId,
        Guid youthId,
        IdeaDiscoveryCategory category,
        int limit)
    {
        string scope = $"workspace={workspaceId}, project={projectId}, topic={topicId}, youth={youthId}, category={category}, limit={limit}";
        if (limit <= 0)
        {
            LogDiscovery(scope, source: "invalid-limit", candidateCount: 0, rankedCount: 0, pickedCount: 0);
            return Array.Empty<Idea>();
        }

        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        _projectManager.GetTopic(project, topicId);

        // Feature requirement: suggestions are only available when the user already posted in this topic.
        IReadOnlyCollection<Idea> ownIdeasInTopic = _repository.ReadIdeasByTopicIdAndYouthId(topicId, youthId);
        if (ownIdeasInTopic.Count == 0)
        {
            LogDiscovery(scope, source: "no-own-idea-in-topic", candidateCount: 0, rankedCount: 0, pickedCount: 0);
            return Array.Empty<Idea>();
        }

        var candidates = _repository.ReadIdeasByTopicIdAndStatus(topicId, ModerationStatus.Approved)
            .Where(idea => idea.Youth?.Id != youthId)
            .Take(DiscoveryCandidateLimit)
            .ToList();

        if (candidates.Count == 0)
        {
            LogDiscovery(scope, source: "no-candidates", candidateCount: 0, rankedCount: 0, pickedCount: 0);
            return Array.Empty<Idea>();
        }

        int cappedLimit = Math.Min(limit, candidates.Count);

        if (category == IdeaDiscoveryCategory.Random)
        {
            var randomPicks = ShuffleIdeas(candidates).Take(cappedLimit).ToList().AsReadOnly();
            LogDiscovery(scope, source: "random", candidateCount: candidates.Count, rankedCount: 0, pickedCount: randomPicks.Count);
            return randomPicks;
        }

        string referenceIdea = ownIdeasInTopic
            .OrderByDescending(idea => idea.SubmissionDate)
            .Select(idea => idea.Content)
            .FirstOrDefault(content => !string.IsNullOrWhiteSpace(content))
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(referenceIdea))
        {
            var fallbackPicks = ShuffleIdeas(candidates).Take(cappedLimit).ToList().AsReadOnly();
            LogDiscovery(scope, source: "fallback-no-reference", candidateCount: candidates.Count, rankedCount: 0, pickedCount: fallbackPicks.Count);
            return fallbackPicks;
        }

        IEnumerable<int> rankedIndexes;
        bool aiCallFailed = false;
        try
        {
            rankedIndexes = await _aiManager.RankIdeasByRelationAsync(
                referenceIdea,
                candidates.Select(idea => idea.Content).ToList().AsReadOnly(),
                category == IdeaDiscoveryCategory.Different,
                cappedLimit,
                workspaceId.Text,
                projectId.Text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IdeaDiscovery] AI ranking failed ({scope}): {ex.Message}");
            aiCallFailed = true;
            rankedIndexes = Array.Empty<int>();
        }

        var pickedIdeas = new List<Idea>(cappedLimit);
        var seenIdeaIds = new HashSet<int>();
        foreach (int index in rankedIndexes)
        {
            if (index < 0 || index >= candidates.Count) continue;

            Idea candidate = candidates[index];
            if (!seenIdeaIds.Add(candidate.Id)) continue;
            pickedIdeas.Add(candidate);
            if (pickedIdeas.Count >= cappedLimit) break;
        }

        string source = aiCallFailed
            ? "fallback-ai-error"
            : (rankedIndexes.Count() > 0 && pickedIdeas.Count == rankedIndexes.Count()
                ? "ai-ranked"
                : (rankedIndexes.Count() > 0 ? "ai-ranked+fallback-fill" : "fallback-empty-ai-ranking"));

        LogDiscovery(scope, source, candidates.Count, rankedIndexes.Count(), pickedIdeas.Count);

        return pickedIdeas.AsReadOnly();
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

    public async Task<ResponseSubmissionResponse> AddResponseAsync(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, string responseText)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Youth author;
        try
        {
            author = _projectManager.GetYouth(project, youthId);
        }
        catch (YouthNotFoundException)
        {
            author = _projectManager.AddYouth(youthId, $"{youthId:N}@local.invalid", project.Id);
        }
        Topic topic = _projectManager.GetTopic(project, topicId);
        Idea idea = GetIdea(topic, ideaId);

        responseText = responseText.Trim();
        ModerationDecision decision = await EvaluateIdeaModerationAsync(responseText, workspaceId.Text, projectId.Text);

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
        Topic topic = _projectManager.GetTopic(project, topicId);
        GetIdea(topic, ideaId);
        return _repository.ReadApprovedResponsesByYouthIdAndIdeaId(ideaId, youthId);
    }

    public IdeaReaction AddIdeaReaction(Slug workspaceId, Slug projectId, int topicId, int ideaId, Guid youthId, string emoji)
    {
        Project project = _projectManager.GetProjectById(workspaceId, projectId);
        Topic topic = _projectManager.GetTopic(project, topicId);
        Idea idea = GetIdea(topic, ideaId);
        Youth author;
        try
        {
            author = _projectManager.GetYouth(project, youthId);
        }
        catch (YouthNotFoundException)
        {
            author = _projectManager.AddYouth(youthId, $"{youthId:N}@local.invalid", project.Id);
        }

        string normalizedEmoji = NormalizeEmoji(emoji);
        
        IdeaReaction existingReaction = _repository.ReadIdeaReactionByIdeaIdAndYouthIdAndEmoji(ideaId, youthId, normalizedEmoji);
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
        Youth author;
        try
        {
            author = _projectManager.GetYouth(project, youthId);
        }
        catch (YouthNotFoundException)
        {
            author = _projectManager.AddYouth(youthId, $"{youthId:N}@local.invalid", project.Id);
        }
        string normalizedEmoji = NormalizeEmoji(emoji);

        ResponseReaction existingReaction = _repository.ReadResponseReactionByResponseIdAndYouthIdAndEmoji(responseId, youthId, normalizedEmoji);
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

    private static IReadOnlyList<Idea> ShuffleIdeas(IReadOnlyList<Idea> ideas)
    {
        var shuffled = ideas.ToList();
        for (int index = shuffled.Count - 1; index > 0; index--)
        {
            int swapIndex = Random.Shared.Next(index + 1);
            (shuffled[index], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[index]);
        }

        return shuffled.AsReadOnly();
    }

    private async Task AssignSemanticCategoriesToIdeaAsync(Idea idea, int topicId, string? workspaceId = null, string? projectId = null)
    {
        string[] categories = { "General ideas" };

        try
        {
            var existingCategories = LoadTopicSemanticCategories(topicId);
            var categorization = await _aiManager
                .CategorizeIdeasAsync(
                    new[] { idea.Content ?? string.Empty }.ToList().AsReadOnly(),
                    existingCategories,
                    MaxCategoriesPerIdea,
                    workspaceId,
                    projectId);

            var rawCategories = categorization.TryGetValue(0, out var assigned)
                ? assigned
                : Array.Empty<string>();

            var canonical = CanonicalizeSemanticCategories(rawCategories, existingCategories)
                .Take(MaxCategoriesPerIdea)
                .ToArray();

            if (canonical.Length > 0)
            {
                categories = canonical;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IdeaDiscovery] Semantic categorization failed for idea {idea.Id}: {ex.Message}");
        }

        idea.SemanticCategories = categories;

        _repository.UpdateIdea(idea);
    }

    private void EnsureSemanticCategoriesForIdeas(IReadOnlyList<Idea> ideas)
    {
        var knownCategories = LoadKnownSemanticCategories(ideas);
        var uncategorized = ideas
            .Where(idea => idea.SemanticCategories == null || idea.SemanticCategories.Length == 0)
            .ToList();

        if (uncategorized.Count == 0)
        {
            return;
        }

        for (int batchStart = 0; batchStart < uncategorized.Count; batchStart += CategorizationBatchSize)
        {
            var batch = uncategorized.Skip(batchStart).Take(CategorizationBatchSize).ToList();
            var batchTexts = batch.Select(idea => idea.Content ?? string.Empty).ToList().AsReadOnly();

            IReadOnlyDictionary<int, IReadOnlyList<string>> categorizedByIndex;
            try
            {
                categorizedByIndex = _aiManager
                    .CategorizeIdeasAsync(batchTexts, knownCategories.AsReadOnly(), MaxCategoriesPerIdea).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IdeaDiscovery] Semantic categorization fallback used: {ex.Message}");
                categorizedByIndex = new Dictionary<int, IReadOnlyList<string>>();
            }

            for (int index = 0; index < batch.Count; index++)
            {
                var categories = categorizedByIndex.TryGetValue(index, out var assigned)
                    ? assigned
                    : Array.Empty<string>();

                var normalized = CanonicalizeSemanticCategories(categories, knownCategories)
                    .Take(MaxCategoriesPerIdea)
                    .ToArray();

                batch[index].SemanticCategories = normalized.Length > 0
                    ? normalized
                    : CanonicalizeSemanticCategories(new[] { "General ideas" }, knownCategories).ToArray();

                RegisterSemanticCategories(knownCategories, batch[index].SemanticCategories);

                _repository.UpdateIdea(batch[index]);
            }
        }
    }

    private static List<string> LoadKnownSemanticCategories(IEnumerable<Idea> ideas)
    {
        var categories = new List<string>();
        RegisterSemanticCategories(categories, ideas.SelectMany(idea => idea.SemanticCategories ?? Array.Empty<string>()));
        return categories;
    }

    private IReadOnlyList<string> LoadTopicSemanticCategories(int topicId)
    {
        var ideas = _repository.ReadIdeasByTopicId(topicId);
        return LoadKnownSemanticCategories(ideas);
    }

    private static IReadOnlyList<string> CanonicalizeSemanticCategories(IEnumerable<string> categories, IReadOnlyList<string> knownCategories)
    {
        var canonical = new List<string>();
        var knownByKey = knownCategories
            .Select(category => (category ?? string.Empty).Trim())
            .Where(category => category.Length > 0)
            .GroupBy(NormalizeCategoryKey)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var raw in categories)
        {
            var trimmed = (raw ?? string.Empty).Trim();
            if (trimmed.Length == 0) continue;

            var key = NormalizeCategoryKey(trimmed);
            var resolved = knownByKey.TryGetValue(key, out var existing) ? existing : trimmed;

            if (canonical.Any(category => NormalizeCategoryKey(category) == key))
            {
                continue;
            }

            canonical.Add(resolved);
            if (!knownByKey.ContainsKey(key))
            {
                knownByKey[key] = resolved;
            }
        }

        return canonical;
    }

    private static void RegisterSemanticCategories(ICollection<string> knownCategories, IEnumerable<string> categories)
    {
        foreach (var category in categories)
        {
            var trimmed = (category ?? string.Empty).Trim();
            if (trimmed.Length == 0) continue;

            if (knownCategories.Any(existing => NormalizeCategoryKey(existing) == NormalizeCategoryKey(trimmed)))
            {
                continue;
            }

            knownCategories.Add(trimmed);
        }
    }

    private static string NormalizeCategoryKey(string value)
    {
        return new string((value ?? string.Empty)
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
    }

    private static void LogDiscovery(string scope, string source, int candidateCount, int rankedCount, int pickedCount)
    {
        Console.WriteLine(
            $"[IdeaDiscovery] source={source}; candidates={candidateCount}; ranked={rankedCount}; picked={pickedCount}; {scope}");
    }

    private async Task<ModerationDecision> EvaluateIdeaModerationAsync(string content, string workspaceId = null, string projectId = null)
    {
        Console.WriteLine($"[IdeaManager] Sending content to moderation: \"{content}\"");
        ModerationDecision fallbackDecision = new ModerationDecision { IsAllowed = true };
        
        try
        {
            var decision = await _aiManager.ModerateContentAsync(content, workspaceId, projectId);

            if (decision.IsAllowed)
            {
                return decision;
            }

            try
            {
                decision.Suggestion = await _aiManager.GenerateAlternativeAsync(content, decision, workspaceId, projectId);
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
