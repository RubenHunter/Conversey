using System.Text.Json;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Ideation;
using Conversey.DAL.Subplatform.Ai;

namespace Conversey.BL.Ai;

public sealed class AiManager : IAiManager
{
    private readonly IAiProvider _provider;
    private readonly IPromptRepository _promptRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IModerationKeywordRepository _moderationKeywordRepository;
    private readonly string _completionsModel;
    private readonly string _moderationModel;
    private readonly decimal _temperature;

    private const string CompletionsModelType = "Completions";
    private const string ModerationModelType = "Moderation";

    private static readonly HashSet<string> ModelSafetyIndicators = new(StringComparer.OrdinalIgnoreCase)
    {
        "unsafe", "not safe", "hate", "toxic", "violent", "harassment",
        "inappropriate", "offensive", "flag", "harmful", "abuse", "dangerous"
    };

    public AiManager(IAiProvider provider, IPromptRepository promptRepository, IAuditRepository auditRepository, IModerationKeywordRepository moderationKeywordRepository, string completionsModel, string moderationModel, decimal temperature = 1.0m)
    {
        _provider = provider;
        _promptRepository = promptRepository;
        _auditRepository = auditRepository;
        _moderationKeywordRepository = moderationKeywordRepository;
        _completionsModel = string.IsNullOrWhiteSpace(completionsModel) ? "mistral-small-latest" : completionsModel;
        _moderationModel = moderationModel;
        _temperature = temperature == 0m ? 1.0m : temperature;
    }

    public async Task<ModerationDecision> ModerateContentAsync(string content)
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

        if (_provider.SupportsNativeModeration && !string.IsNullOrWhiteSpace(_moderationModel))
        {
            Console.WriteLine($"[AiManager] → Native moderation endpoint (/{_moderationModel})");
            return await ModerateViaNativeEndpointAsync(content);
        }

        var effectiveModel = string.IsNullOrWhiteSpace(_moderationModel) ? _completionsModel : _moderationModel;
        Console.WriteLine($"[AiManager] → Prompt-based moderation (model: \"{effectiveModel}\")");
        return await ModerateViaPromptAsync(content);
    }

    private async Task<ModerationDecision> ModerateViaNativeEndpointAsync(string content)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var result = await _provider.ModerateAsync(content, _moderationModel);
            var duration = DateTime.UtcNow - startTime;

            await _auditRepository.LogAiCallAsync(_moderationModel, ModerationModelType, 0, 0, 0, startTime, duration, _provider.ProviderName, "Moderation");

            var info = ParseModerationInfo(result.Categories);
            var isAllowed = !result.Flagged && !HasAnyModerationFlag(info);

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

    private async Task<ModerationDecision> ModerateViaPromptAsync(string content)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var prompt = await LoadPromptAsync("ModerationPrompt");
            var systemPrompt = string.IsNullOrWhiteSpace(prompt.SystemPrompt)
                ? BuildDefaultModerationSystemPrompt()
                : prompt.SystemPrompt;

            var model = string.IsNullOrWhiteSpace(_moderationModel) ? _completionsModel : _moderationModel;
            var result = await _provider.CompleteAsync(systemPrompt, content, model, 0m);
            var duration = DateTime.UtcNow - startTime;

            await _auditRepository.LogAiCallAsync(model, ModerationModelType, result.PromptTokens, result.CompletionTokens, 0, startTime, duration, _provider.ProviderName, "ModerationPrompt");

            if (string.IsNullOrWhiteSpace(result.Content))
            {
                return new ModerationDecision { IsAllowed = true };
            }

            Console.WriteLine($"[AiManager] Moderation prompt response ({result.CompletionTokens} tokens): \"{result.Content.Trim()}\"");
            var parsed = ParseModerationPromptResponse(result.Content);
            return parsed;
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

    public async Task<string> GenerateAlternativeAsync(string content, ModerationDecision decision = null)
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

            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, 0, startTime, duration, _provider.ProviderName, "ModerationGenerateAlternative");

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

    public async Task<IdeaNudgeDecision> AssessIdeaNudgeAsync(IdeaNudgeAssessmentRequest request)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var systemPrompt = await LoadPromptAsync("IdeaNudgingSystem");
            var systemContent = string.IsNullOrWhiteSpace(systemPrompt.SystemPrompt)
                ? BuildDefaultNudgingSystemPrompt(request.NudgingMode)
                : PromptRenderer.Render(systemPrompt.SystemPrompt, new Dictionary<string, string> { ["NudgingModeDescription"] = DescribeNudgingMode(request.NudgingMode) });

            var userPrompt = await LoadPromptAsync("IdeaNudgingUser");
            var userContent = string.IsNullOrWhiteSpace(userPrompt.UserPromptTemplate)
                ? BuildDefaultNudgingUserPrompt(request)
                : PromptRenderer.Render(userPrompt.UserPromptTemplate, BuildNudgingVariables(request));

            var result = await _provider.CompleteAsync(systemContent, userContent, _completionsModel, _temperature);
            var duration = DateTime.UtcNow - startTime;

            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, 0, startTime, duration, _provider.ProviderName, "IdeaNudging");

            if (string.IsNullOrWhiteSpace(result.Content))
            {
                return new IdeaNudgeDecision { IsApproved = true };
            }

            return ParseNudgeDecision(result.Content);
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

    public async Task<IEnumerable<int>> RankIdeasByRelationAsync(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
    {
        if (candidateIdeas.Count == 0 || limit <= 0)
        {
            return Array.Empty<int>();
        }

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
                ? BuildDefaultIdeaRankingPrompt(referenceIdea, candidateIdeas, preferDifferent, cappedLimit)
                : PromptRenderer.Render(userPrompt.UserPromptTemplate, BuildRankingVariables(referenceIdea, candidateIdeas, preferDifferent, cappedLimit));

            var result = await _provider.CompleteAsync(systemContent, userContent, _completionsModel, _temperature);
            var duration = DateTime.UtcNow - startTime;

            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, 0, startTime, duration, _provider.ProviderName, "IdeaRanking");

            if (string.IsNullOrWhiteSpace(result.Content))
            {
                return Array.Empty<int>();
            }

            return ParseRankedIndexes(result.Content, candidateIdeas.Count, cappedLimit);
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
        int maxCategoriesPerIdea)
    {
        if (ideas.Count == 0)
        {
            return new Dictionary<int, IReadOnlyList<string>>();
        }

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
                ? BuildDefaultCategorizationPrompt(ideas, existingCategories, cappedMax)
                : PromptRenderer.Render(userPrompt.UserPromptTemplate, BuildCategorizationVariables(ideas, existingCategories, cappedMax));

            var result = await _provider.CompleteAsync(systemContent, userContent, _completionsModel, _temperature);
            var duration = DateTime.UtcNow - startTime;

            await _auditRepository.LogAiCallAsync(_completionsModel, CompletionsModelType, result.PromptTokens, result.CompletionTokens, 0, startTime, duration, _provider.ProviderName, "IdeaCategorization");

            if (string.IsNullOrWhiteSpace(result.Content))
            {
                return new Dictionary<int, IReadOnlyList<string>>();
            }

            return ParseCategorizedIdeas(result.Content, ideas.Count, cappedMax);
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

    private async Task<AiPrompt> LoadPromptAsync(string name)
    {
        var prompt = await _promptRepository.GetPromptAsync(name);
        return prompt ?? new AiPrompt { Name = name };
    }

    private static IReadOnlyDictionary<string, string> BuildNudgingVariables(IdeaNudgeAssessmentRequest request)
    {
        var conversation = request.Conversation.Count == 0
            ? "(no previous nudge questions yet)"
            : string.Join("\n", request.Conversation.Select((turn, index) => $"Turn {index + 1} question: {turn.Question}\nTurn {index + 1} answer: {turn.Answer}"));

        return new Dictionary<string, string>
        {
            ["ProjectTitle"] = request.ProjectTitle ?? string.Empty,
            ["ProjectDescription"] = request.ProjectDescription ?? string.Empty,
            ["TopicTitle"] = request.TopicTitle ?? string.Empty,
            ["TopicPrompt"] = request.TopicPrompt ?? string.Empty,
            ["IdeaText"] = request.IdeaText ?? string.Empty,
            ["Conversation"] = conversation
        };
    }

    private static IReadOnlyDictionary<string, string> BuildRankingVariables(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
    {
        var relationGoal = preferDifferent
            ? "Return ideas that take a noticeably different angle, theme, or approach than the reference idea. Include ideas with a different focus or perspective, not just extreme opposites. Skip only ideas that are nearly identical in meaning to the reference."
            : "Return ideas that share a similar theme, goal, or approach with the reference idea. Skip ideas that are clearly unrelated or focused on a completely different topic.";

        var candidates = string.Join("\n", candidateIdeas.Select((idea, index) => $"[{index}] {idea}"));

        return new Dictionary<string, string>
        {
            ["ReferenceIdea"] = referenceIdea ?? string.Empty,
            ["Candidates"] = candidates,
            ["RelationGoal"] = relationGoal,
            ["Limit"] = limit.ToString()
        };
    }

    private static IReadOnlyDictionary<string, string> BuildCategorizationVariables(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
    {
        var indexedIdeas = string.Join("\n", ideas.Select((idea, index) => $"[{index}] {idea}"));
        var existingCategoryList = existingCategories.Count == 0
            ? "(none yet)"
            : string.Join(", ", existingCategories.Distinct(StringComparer.OrdinalIgnoreCase));

        return new Dictionary<string, string>
        {
            ["Ideas"] = indexedIdeas,
            ["ExistingCategories"] = existingCategoryList,
            ["MaxCategoriesPerIdea"] = maxCategoriesPerIdea.ToString()
        };
    }

    private static string BuildDefaultNudgingSystemPrompt(string nudgingMode)
    {
        return $"You help youth improve the quality of their idea before publishing. Ask exactly one concrete follow-up question when the idea is too shallow, vague, or underspecified. If the idea is already acceptable for the configured nudging strength, approve it. Never invent multiple questions. Return strict JSON only with the shape {{\"isApproved\":true}} or {{\"isApproved\":false,\"question\":\"...\"}}. Nudging strength: {DescribeNudgingMode(nudgingMode)}.";
    }

    private static string BuildDefaultNudgingUserPrompt(IdeaNudgeAssessmentRequest request)
    {
        var conversation = request.Conversation.Count == 0
            ? "(no previous nudge questions yet)"
            : string.Join("\n", request.Conversation.Select((turn, index) => $"Turn {index + 1} question: {turn.Question}\nTurn {index + 1} answer: {turn.Answer}"));

        return $$"""
Project title: {{request.ProjectTitle}}
Project description: {{request.ProjectDescription}}
Topic title: {{request.TopicTitle}}
Topic prompt/question: {{request.TopicPrompt}}

Current idea draft:
{{request.IdeaText}}

Conversation so far:
{{conversation}}

Decide whether the draft is ready. If not, ask one follow-up question that is specific to this idea and helps deepen it using the project and topic context.
""";
    }

    private static string DescribeNudgingMode(string nudgingMode)
    {
        return (nudgingMode ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "minimal" or "verylenient" or "lenient" or "acceptable" => "Minimal: accept anything that is a real sentence; reject empty or obvious placeholder text.",
            "light" or "gentle" => "Light: require a concrete subject; ask at most one clarifying question when needed.",
            "medium" or "balanced" or "guided" => "Medium: require what + why; ask focused follow-ups when depth is missing.",
            "strong" or "strict" or "thorough" => "Strong: require context or impact and concrete relevance to topic.",
            "deep" or "relentless" => "Deep: challenge assumptions and ask for evidence or concrete elaboration.",
            _ => "Medium: require what + why; ask focused follow-ups when depth is missing."
        };
    }

    private static string BuildDefaultIdeaRankingPrompt(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
    {
        var relationGoal = preferDifferent
            ? "Return ideas that take a noticeably different angle, theme, or approach than the reference idea. Include ideas with a different focus or perspective, not just extreme opposites. Skip only ideas that are nearly identical in meaning to the reference."
            : "Return ideas that share a similar theme, goal, or approach with the reference idea. Skip ideas that are clearly unrelated or focused on a completely different topic.";

        var candidates = string.Join("\n", candidateIdeas.Select((idea, index) => $"[{index}] {idea}"));

        return $$"""
Reference idea:
{{referenceIdea}}

Candidate ideas (use only these indexes):
{{candidates}}

Task:
- {{relationGoal}}
- Return up to {{limit}} indexes, ordered from best to least fitting for this relation.
- Do not invent indexes.
- Return strict JSON only with this schema:
{"rankedIndexes":[0,1,2]}
""";
    }

    private static string BuildDefaultCategorizationPrompt(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
    {
        var indexedIdeas = string.Join("\n", ideas.Select((idea, index) => $"[{index}] {idea}"));
        var existingCategoryList = existingCategories.Count == 0
            ? "(none yet)"
            : string.Join(", ", existingCategories.Distinct(StringComparer.OrdinalIgnoreCase));
        return $$"""
Categorize each idea semantically. One idea may belong to multiple categories.

These are the existing categories already used in this topic. Reuse these exact labels whenever possible and only invent a new label if nothing fits:
{{existingCategoryList}}

Ideas:
{{indexedIdeas}}

Rules:
- Use short, human-readable category names.
- Max {{maxCategoriesPerIdea}} categories per idea.
- Prefer reusing an existing category label when it is semantically close enough.
- Avoid near-duplicate labels when an existing category already covers the same meaning.
- Do not invent idea indexes.
- Avoid creating near-duplicate labels if an existing category already fits.
- Return strict JSON only in this shape:
{"items":[{"index":0,"categories":["Category A","Category B"]}]}
""";
    }

    private static ModerationInfo ParseModerationInfo(IReadOnlyDictionary<string, bool> categories)
    {
        return new ModerationInfo
        {
            Sexual = GetFlag(categories, "sexual"),
            HateAndDiscrimination = GetFlag(categories, "hate_and_discrimination") || GetFlag(categories, "hate"),
            ViolenceAndThreats = GetFlag(categories, "violence_and_threats") || GetFlag(categories, "violence"),
            DangerousAndCriminalContent = GetFlag(categories, "dangerous_and_criminal_content"),
            SelfHarm = GetFlag(categories, "self_harm"),
            Pii = GetFlag(categories, "pii")
        };
    }

    private static bool GetFlag(IReadOnlyDictionary<string, bool> categories, string key)
    {
        return categories.TryGetValue(key, out var value) && value;
    }

    private static bool HasAnyModerationFlag(ModerationInfo info)
    {
        return info.Sexual ||
               info.HateAndDiscrimination ||
               info.ViolenceAndThreats ||
               info.DangerousAndCriminalContent ||
               info.SelfHarm ||
               info.Pii;
    }

    private static string BuildDefaultModerationSystemPrompt()
    {
        return """
You are a strict content safety classifier for a youth platform. Your task is to flag ANY harmful, toxic, or unsafe content.

Analyze the text against these categories:
- sexual: sexually explicit content, sexual harassment, or sexualized language
- hate_and_discrimination: slurs, hate speech, racism, homophobia, transphobia, bigotry, or discrimination based on identity
- violence_and_threats: threats of violence, encouragement of violence, or glorification of harm
- dangerous_and_criminal_content: illegal activity, self-harm instructions, or dangerous pranks
- self_harm: content promoting or encouraging self-harm or suicide
- pii: personal identifiable information like phone numbers, addresses, or full names

Also mark hate_and_discrimination as true for: personal insults involving slurs, name-calling with protected characteristics, profanity-laced harassment, hostile derogatory language, or general offensive/crude language targeting others.

CRITICAL: Be conservative. If you are unsure whether content violates a category, mark it as violating. False positives are safer than false negatives.

Return ONLY a JSON object with this exact schema:
{"flagged":true,"categories":{"sexual":false,"hate_and_discrimination":true,"violence_and_threats":false,"dangerous_and_criminal_content":false,"self_harm":false,"pii":false}}

No markdown, no code blocks, no explanation — just the raw JSON.
""";
    }

    private static ModerationDecision ParseModerationPromptResponse(string rawContent)
    {
        string json = rawContent.Trim();

        int fenceStart = json.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (fenceStart >= 0)
        {
            int fenceEnd = json.IndexOf("```", fenceStart + 7);
            if (fenceEnd > fenceStart)
            {
                json = json.Substring(fenceStart + 7, fenceEnd - fenceStart - 7).Trim();
            }
        }
        else if (json.StartsWith("```"))
        {
            int fenceEnd = json.IndexOf("```", 3);
            if (fenceEnd > 0)
            {
                json = json.Substring(3, fenceEnd - 3).Trim();
            }
        }

        int firstBrace = json.IndexOf('{');
        int lastBrace = json.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            json = json.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        try
        {
            using var parsed = JsonDocument.Parse(json);
            var root = parsed.RootElement;

            var flagged = root.TryGetProperty("flagged", out var flaggedElement) && flaggedElement.ValueKind == JsonValueKind.True;

            var categories = new Dictionary<string, bool>();
            if (root.TryGetProperty("categories", out var cats) && cats.ValueKind == JsonValueKind.Object)
            {
                foreach (var cat in cats.EnumerateObject())
                {
                    categories[cat.Name] = cat.Value.ValueKind == JsonValueKind.True;
                }
            }

            var info = ParseModerationInfo(categories);
            var isAllowed = !flagged && !HasAnyModerationFlag(info);

            return new ModerationDecision { IsAllowed = isAllowed, Categories = info };
        }
        catch (JsonException)
        {
            Console.WriteLine($"[AiManager] Moderation prompt response was not valid JSON. Raw: \"{rawContent.Trim()}\"");

            var hasUnsafeKeyword = ModelSafetyIndicators.Any(term =>
                rawContent.Contains(term, StringComparison.OrdinalIgnoreCase));
            if (hasUnsafeKeyword)
            {
                return new ModerationDecision { IsAllowed = false };
            }

            return new ModerationDecision { IsAllowed = true };
        }
    }

    private static IdeaNudgeDecision ParseNudgeDecision(string rawContent)
    {
        string json = rawContent.Trim();
        int firstBrace = json.IndexOf('{');
        int lastBrace = json.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            json = json.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        try
        {
            using var parsed = JsonDocument.Parse(json);
            var root = parsed.RootElement;
            var decision = new IdeaNudgeDecision
            {
                IsApproved = GetBoolean(root, "isApproved") || GetBoolean(root, "approved")
            };

            if (!decision.IsApproved && root.TryGetProperty("question", out var questionElement) && questionElement.ValueKind == JsonValueKind.String)
            {
                decision.Question = questionElement.GetString()?.Trim();
            }

            if (string.IsNullOrWhiteSpace(decision.Question) && !decision.IsApproved)
            {
                decision.Question = "Can you make this idea more specific for this topic?";
            }

            if (decision.IsApproved)
            {
                decision.Question = null;
            }

            return decision;
        }
        catch (JsonException ex)
        {
            throw new AiException("Invalid AI response for idea nudging", ex);
        }
    }

    private static IReadOnlyList<int> ParseRankedIndexes(string rawContent, int candidateCount, int limit)
    {
        string json = rawContent.Trim();
        int firstBrace = json.IndexOf('{');
        int lastBrace = json.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            json = json.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        try
        {
            using var parsed = JsonDocument.Parse(json);
            if (!parsed.RootElement.TryGetProperty("rankedIndexes", out var indexesElement) || indexesElement.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<int>();
            }

            var picked = new List<int>();
            var seen = new HashSet<int>();
            foreach (var value in indexesElement.EnumerateArray())
            {
                if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int index)) continue;
                if (index < 0 || index >= candidateCount || !seen.Add(index)) continue;
                picked.Add(index);
                if (picked.Count >= limit) break;
            }

            return picked.AsReadOnly();
        }
        catch (JsonException ex)
        {
            throw new AiRankingException("Invalid AI response", ex);
        }
    }

    private static IReadOnlyDictionary<int, IReadOnlyList<string>> ParseCategorizedIdeas(string rawContent, int ideaCount, int maxCategoriesPerIdea)
    {
        var result = new Dictionary<int, IReadOnlyList<string>>();

        string json = rawContent.Trim();
        int firstBrace = json.IndexOf('{');
        int lastBrace = json.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            json = json.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        try
        {
            using var parsed = JsonDocument.Parse(json);
            if (!parsed.RootElement.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var item in itemsElement.EnumerateArray())
            {
                if (!item.TryGetProperty("index", out var indexElement)) continue;

                int index;

                if (indexElement.ValueKind == JsonValueKind.Number)
                {
                    if (!indexElement.TryGetInt32(out index)) continue;
                }
                else if (indexElement.ValueKind == JsonValueKind.String)
                {
                    if (!int.TryParse(indexElement.GetString(), out index)) continue;
                }
                else
                {
                    continue;
                }

                if (index < 0 || index >= ideaCount) continue;

                if (!item.TryGetProperty("categories", out var categoriesElement) ||
                    categoriesElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var categories = categoriesElement
                    .EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString()?.Trim() ?? string.Empty)
                    .Where(c => c.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(maxCategoriesPerIdea)
                    .ToList();

                if (categories.Count > 0)
                {
                    result[index] = categories.AsReadOnly();
                }
            }

            return result;
        }
        catch (JsonException)
        {
            return result;
        }
    }

    private static bool GetBoolean(JsonElement obj, string property)
    {
        if (!obj.TryGetProperty(property, out var value)) return false;
        return value.ValueKind == JsonValueKind.True;
    }
}
