using Conversey.BL.Domain.Ai;
using Conversey.BL.Ai.DTOs;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Subplatform.Ai;

namespace Conversey.BL.Ai;

public sealed class AiManager : IAiManager
{
    private readonly IAiProvider _provider;
    private readonly IPromptRepository _promptRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IModerationKeywordRepository _moderationKeywordRepository;
    private readonly IAiPricingService _pricingService;
    private readonly string _completionsModel;
    private readonly string _moderationModel;
    private readonly decimal _temperature;

    private const string CompletionsModelType = "Completions";
    private const string ModerationModelType = "Moderation";

    public AiManager(IAiProvider provider, IPromptRepository promptRepository, IAuditRepository auditRepository, IModerationKeywordRepository moderationKeywordRepository, IAiPricingService pricingService, string completionsModel, string moderationModel, decimal temperature = 1.0m)
    {
        _provider = provider;
        _promptRepository = promptRepository;
        _auditRepository = auditRepository;
        _moderationKeywordRepository = moderationKeywordRepository;
        _pricingService = pricingService;
        _completionsModel = string.IsNullOrWhiteSpace(completionsModel) ? "mistral-small-latest" : completionsModel;
        _moderationModel = moderationModel;
        _temperature = temperature == 0m ? 1.0m : temperature;
    }

    public async Task<ModerationDecision> ModerateContentAsync(string content, string? workspaceId = null, string? projectId = null)
    {
        var keywordSet = _moderationKeywordRepository.GetKeywordSet();
        var unsafeTerm = keywordSet.FirstOrDefault(term => (content ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase));
        if (unsafeTerm != null)
        {
            var preview = (content ?? string.Empty).Length > 120 ? (content ?? string.Empty)[..120] + "..." : content;
            Console.WriteLine($"[AiManager] Keyword filter rejected content (matched: \"{unsafeTerm}\"): \"{preview}\"");
            return new ModerationDecision { IsAllowed = false };
        }

        Console.WriteLine($"[AiManager] Keywords passed. Provider={_provider.ProviderName}, NativeModeration={_provider.SupportsNativeModeration}, ModerationModel=\"{_moderationModel}\", CompletionsModel=\"{_completionsModel}\"");

        try
        {
            if (_provider.SupportsNativeModeration && !string.IsNullOrWhiteSpace(_moderationModel))
            {
                Console.WriteLine($"[AiManager] → Native moderation endpoint (/{_moderationModel})");
                return await ModerateViaNativeEndpointAsync(content, workspaceId, projectId);
            }

            var effectiveModel = string.IsNullOrWhiteSpace(_moderationModel) ? _completionsModel : _moderationModel;
            Console.WriteLine($"[AiManager] → Prompt-based moderation (model: \"{effectiveModel}\")");
            return await ModerateViaPromptAsync(content, workspaceId, projectId);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
            Console.WriteLine($"[AiManager] Moderation provider returned 503 — marking for review");
            return new ModerationDecision { IsAllowed = false };
        }
        catch (AiException ex) when (ex.Message.Contains("503", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[AiManager] Moderation provider unavailable (503) — marking for review");
            return new ModerationDecision { IsAllowed = false };
        }
        catch (HttpRequestException)
        {
            Console.WriteLine($"[AiManager] Moderation provider unreachable (network) — marking for review");
            return new ModerationDecision { IsAllowed = false };
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"[AiManager] Moderation provider timed out — marking for review");
            return new ModerationDecision { IsAllowed = false };
        }
    }

    private async Task<ModerationDecision> ModerateViaNativeEndpointAsync(string content, string? workspaceId = null, string? projectId = null)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var result = await _provider.ModerateAsync(content, _moderationModel);
            var duration = DateTime.UtcNow - startTime;

            await _auditRepository.LogAiCallAsync(_moderationModel, ModerationModelType, 0, 0, 0, startTime, duration, _provider.ProviderName, "Moderation", workspaceId, projectId);

            var info = AiResponseParser.ParseModerationInfo(result.Categories);
            var isAllowed = !result.Flagged && !AiResponseParser.HasAnyModerationFlag(info);

            return new ModerationDecision { IsAllowed = isAllowed, Categories = info };
        }
        catch (AiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiException("Moderation failed", ex);
        }
    }

    private async Task<ModerationDecision> ModerateViaPromptAsync(string content, string? workspaceId = null, string? projectId = null)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var prompt = await LoadPromptAsync("ModerationPrompt");
            var systemPrompt = string.IsNullOrWhiteSpace(prompt.SystemPrompt)
                ? AiPromptDefaults.BuildModerationSystemPrompt()
                : prompt.SystemPrompt;

            var model = string.IsNullOrWhiteSpace(_moderationModel) ? _completionsModel : _moderationModel;
            var result = await _provider.CompleteAsync(systemPrompt, content, model, 0m);
            var duration = DateTime.UtcNow - startTime;

            var cost = await ComputeCostAsync(model, result.PromptTokens, result.CompletionTokens);
            await _auditRepository.LogAiCallAsync(model, ModerationModelType, result.PromptTokens, result.CompletionTokens, cost, startTime, duration, _provider.ProviderName, "ModerationPrompt", workspaceId, projectId);

            if (string.IsNullOrWhiteSpace(result.Content))
                return new ModerationDecision { IsAllowed = true };

            Console.WriteLine($"[AiManager] Moderation prompt response ({result.CompletionTokens} tokens): \"{result.Content.Trim()}\"");
            return AiResponseParser.ParseModerationPromptResponse(result.Content);
        }
        catch (AiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiException("Prompt-based moderation failed", ex);
        }
    }

    public async Task<string> GenerateAlternativeAsync(string content, ModerationDecision decision = null, string? workspaceId = null, string? projectId = null)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var prompt = await LoadPromptAsync("ModerationGenerateAlternative");
            var systemPrompt = string.IsNullOrWhiteSpace(prompt.SystemPrompt)
                ? "You rewrite unsafe user feedback into respectful, constructive feedback while preserving intent. Return only the rewritten text."
                : prompt.SystemPrompt;

            var result = await _provider.CompleteAsync(systemPrompt, content, _completionsModel, _temperature);
            var duration = DateTime.UtcNow - startTime;

            var cost = await ComputeCostAsync(_completionsModel, result.PromptTokens, result.CompletionTokens);
            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, cost, startTime, duration, _provider.ProviderName, "ModerationGenerateAlternative", workspaceId, projectId);

            return !string.IsNullOrWhiteSpace(result.Content) ? result.Content : "Please rephrase your message in a respectful way.";
        }
        catch (AiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiException("Alternative generation failed", ex);
        }
    }

    public async Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(IdeaNudgeAssessmentRequest request, string? workspaceId = null, string? projectId = null)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var systemPrompt = await LoadPromptAsync("IdeaNudgingSystem");
            var systemContent = string.IsNullOrWhiteSpace(systemPrompt.SystemPrompt)
                ? AiPromptDefaults.BuildNudgingSystemPrompt(request.NudgingMode)
                : PromptRenderer.Render(systemPrompt.SystemPrompt, new Dictionary<string, string> { ["NudgingModeDescription"] = AiPromptDefaults.DescribeNudgingMode(request.NudgingMode) });

            var userPrompt = await LoadPromptAsync("IdeaNudgingUser");
            var userContent = string.IsNullOrWhiteSpace(userPrompt.UserPromptTemplate)
                ? AiPromptDefaults.BuildNudgingUserPrompt(request)
                : PromptRenderer.Render(userPrompt.UserPromptTemplate, AiPromptDefaults.BuildNudgingVariables(request));

            var result = await _provider.CompleteAsync(systemContent, userContent, _completionsModel, _temperature);
            var duration = DateTime.UtcNow - startTime;

            var cost = await ComputeCostAsync(_completionsModel, result.PromptTokens, result.CompletionTokens);
            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, cost, startTime, duration, _provider.ProviderName, "IdeaNudging", workspaceId, projectId);

            if (string.IsNullOrWhiteSpace(result.Content))
                return new IdeaNudgeDecision { IsApproved = true };

            return AiResponseParser.ParseNudgeDecision(result.Content);
        }
        catch (AiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiException("Idea nudging failed", ex);
        }
    }

    public async Task<IEnumerable<int>> RankIdeasByRelationAsync(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit, string? workspaceId = null, string? projectId = null)
    {
        if (candidateIdeas.Count == 0 || limit <= 0)
            return Array.Empty<int>();

        int cappedLimit = Math.Min(limit, candidateIdeas.Count);
        var startTime = DateTime.UtcNow;
        try
        {
            var systemPrompt = await LoadPromptAsync("IdeaRankingSystem");
            var systemContent = string.IsNullOrWhiteSpace(systemPrompt.SystemPrompt)
                ? "You compare youth ideas by meaning. Return only strict JSON with field rankedIndexes as an array of integer indexes. For similarity tasks, return clearly similar ideas. For difference tasks, return ideas with a noticeably different focus or approach; be inclusive rather than restrictive."
                : systemPrompt.SystemPrompt;

            var userPrompt = await LoadPromptAsync("IdeaRankingUser");
            var userContent = string.IsNullOrWhiteSpace(userPrompt.UserPromptTemplate)
                ? AiPromptDefaults.BuildIdeaRankingPrompt(referenceIdea, candidateIdeas, preferDifferent, cappedLimit)
                : PromptRenderer.Render(userPrompt.UserPromptTemplate, AiPromptDefaults.BuildRankingVariables(referenceIdea, candidateIdeas, preferDifferent, cappedLimit));

            var result = await _provider.CompleteAsync(systemContent, userContent, _completionsModel, _temperature);
            var duration = DateTime.UtcNow - startTime;

            var cost = await ComputeCostAsync(_completionsModel, result.PromptTokens, result.CompletionTokens);
            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, cost, startTime, duration, _provider.ProviderName, "IdeaRanking", workspaceId, projectId);

            if (string.IsNullOrWhiteSpace(result.Content))
                return Array.Empty<int>();

            return AiResponseParser.ParseRankedIndexes(result.Content, candidateIdeas.Count, cappedLimit);
        }
        catch (AiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiException("Idea ranking failed", ex);
        }
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeasAsync(
        IReadOnlyList<string> ideas,
        IReadOnlyList<string> existingCategories,
        int maxCategoriesPerIdea,
        string? workspaceId = null,
        string? projectId = null)
    {
        if (ideas.Count == 0)
            return new Dictionary<int, IReadOnlyList<string>>();

        int cappedMax = Math.Clamp(maxCategoriesPerIdea, 1, 4);
        var startTime = DateTime.UtcNow;
        try
        {
            var systemPrompt = await LoadPromptAsync("IdeaCategorizationSystem");
            var systemContent = string.IsNullOrWhiteSpace(systemPrompt.SystemPrompt)
                ? "You assign semantic categories to youth ideas. Return only strict JSON."
                : systemPrompt.SystemPrompt;

            var userPrompt = await LoadPromptAsync("IdeaCategorizationUser");
            var userContent = string.IsNullOrWhiteSpace(userPrompt.UserPromptTemplate)
                ? AiPromptDefaults.BuildCategorizationPrompt(ideas, existingCategories, cappedMax)
                : PromptRenderer.Render(userPrompt.UserPromptTemplate, AiPromptDefaults.BuildCategorizationVariables(ideas, existingCategories, cappedMax));

            var result = await _provider.CompleteAsync(systemContent, userContent, _completionsModel, _temperature);
            var duration = DateTime.UtcNow - startTime;

            var cost = await ComputeCostAsync(_completionsModel, result.PromptTokens, result.CompletionTokens);
            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, cost, startTime, duration, _provider.ProviderName, "IdeaCategorization", workspaceId, projectId);

            if (string.IsNullOrWhiteSpace(result.Content))
                return new Dictionary<int, IReadOnlyList<string>>();

            return AiResponseParser.ParseCategorizedIdeas(result.Content, ideas.Count, cappedMax);
        }
        catch (AiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiException("Idea categorization failed", ex);
        }
    }

    public async Task<ExtractKeyPhrasesResponse> ExtractKeyPhrases(
        string transcript,
        string language,
        int maxPhrases,
        IReadOnlyList<string> existingPhrases = null,
        IReadOnlyList<string> rejectedPhrases = null)
    {
        if (string.IsNullOrWhiteSpace(transcript) || maxPhrases <= 0)
            return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());

        var variables = AiPromptDefaults.BuildKeyPhrasesVariables(transcript, language, maxPhrases, existingPhrases, rejectedPhrases);

        var systemPrompt = await LoadPromptAsync("ExtractKeyPhrasesSystem");
        var systemContent = string.IsNullOrWhiteSpace(systemPrompt.SystemPrompt)
            ? PromptRenderer.Render(AiPromptDefaults.BuildKeyPhrasesSystemPrompt(), variables)
            : PromptRenderer.Render(systemPrompt.SystemPrompt, variables);

        var userPrompt = await LoadPromptAsync("ExtractKeyPhrasesUser");
        var userContent = string.IsNullOrWhiteSpace(userPrompt.UserPromptTemplate)
            ? AiPromptDefaults.BuildKeyPhrasesUserPrompt(transcript, language, maxPhrases, existingPhrases, rejectedPhrases)
            : PromptRenderer.Render(userPrompt.UserPromptTemplate, variables);

        var startTime = DateTime.UtcNow;
        try
        {
            var result = await _provider.CompleteAsync(systemContent, userContent, _completionsModel, 0.1m);
            var duration = DateTime.UtcNow - startTime;

            var cost = await ComputeCostAsync(_completionsModel, result.PromptTokens, result.CompletionTokens);
            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, cost, startTime, duration, _provider.ProviderName, "ExtractKeyPhrases");

            if (string.IsNullOrWhiteSpace(result.Content))
                return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());

            return AiResponseParser.ParseKeyPhrasesResponse(result.Content, existingPhrases, rejectedPhrases, maxPhrases);
        }
        catch (AiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiException("Key phrase extraction failed", ex);
        }
    }

    public async Task<string> GenerateTextFromBubbles(
        string transcript,
        IReadOnlyList<string> bubbles,
        string language,
        IReadOnlyList<string> rejectedPhrases = null)
    {
        if (string.IsNullOrWhiteSpace(transcript) || bubbles == null || bubbles.Count == 0)
            return string.Empty;

        var variables = AiPromptDefaults.BuildTextFromBubblesVariables(transcript, bubbles, language, rejectedPhrases);

        var systemPrompt = await LoadPromptAsync("GenerateTextFromBubblesSystem");
        var systemContent = string.IsNullOrWhiteSpace(systemPrompt.SystemPrompt)
            ? PromptRenderer.Render(AiPromptDefaults.BuildTextFromBubblesSystemPrompt(), variables)
            : PromptRenderer.Render(systemPrompt.SystemPrompt, variables);

        var userPrompt = await LoadPromptAsync("GenerateTextFromBubblesUser");
        var userContent = string.IsNullOrWhiteSpace(userPrompt.UserPromptTemplate)
            ? AiPromptDefaults.BuildTextFromBubblesUserPrompt(transcript, bubbles, language, rejectedPhrases)
            : PromptRenderer.Render(userPrompt.UserPromptTemplate, variables);

        var startTime = DateTime.UtcNow;
        try
        {
            var result = await _provider.CompleteAsync(systemContent, userContent, _completionsModel, 0.3m);
            var duration = DateTime.UtcNow - startTime;

            var cost = await ComputeCostAsync(_completionsModel, result.PromptTokens, result.CompletionTokens);
            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, cost, startTime, duration, _provider.ProviderName, "GenerateTextFromBubbles");

            return !string.IsNullOrWhiteSpace(result.Content) ? result.Content.Trim() : string.Empty;
        }
        catch (AiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiException("Text generation from bubbles failed", ex);
        }
    }

    private async Task<AiPrompt> LoadPromptAsync(string name)
    {
        var prompt = await _promptRepository.GetPromptAsync(name);
        return prompt ?? new AiPrompt { Name = name };
    }

    private async Task<decimal> ComputeCostAsync(string modelName, int inputTokens, int outputTokens)
    {
        if (inputTokens == 0 && outputTokens == 0) return 0m;
        try
        {
            return await _pricingService.CalculateCostAsync(modelName, inputTokens, outputTokens);
        }
        catch
        {
            return 0m;
        }
    }
}
