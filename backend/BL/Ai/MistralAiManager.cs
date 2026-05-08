using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.DTOs.MagicMode;
using Conversey.BL.Domain.Ideation;
using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public sealed class MistralAiManager : IAiManager
{
    private readonly HttpClient _httpClient;
    private readonly string _completionsModel;
    private readonly string _moderationModel;
    private readonly string _keyPhraseModel;

    public MistralAiManager(HttpClient httpClient, AiManagerConfig config)
    {
        _httpClient = httpClient;
        _completionsModel = string.IsNullOrWhiteSpace(config.CompletionsModel) ? "mistral-small-latest" : config.CompletionsModel;
        _moderationModel = string.IsNullOrWhiteSpace(config.ModerationModel) ? "mistral-moderation-latest" : config.ModerationModel;
        _keyPhraseModel = string.IsNullOrWhiteSpace(config.KeyPhraseModel) ? "mistral-small-latest" : config.KeyPhraseModel;
    }

    public void Dispose()
    {
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, "Mistral chat client bridge is not used in this flow."));
        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public object GetService(Type serviceType, object serviceKey = null)
    {
        return null;
    }

    public async Task<ModerationDecision> ModerateContent(string content)
    {
        var payload = new
        {
            model = _moderationModel,
            input = content
        };

        using var response = await _httpClient.PostAsJsonAsync("moderations", payload);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new AiException($"Mistral moderation failed ({(int)response.StatusCode}): {body}", null);
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (!root.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0)
        {
            throw new AiException("Mistral moderation response did not contain results.", null);
        }

        var first = results[0];
        var info = ParseModerationInfo(first);

        var flagged = first.TryGetProperty("flagged", out var flaggedElement) && flaggedElement.ValueKind == JsonValueKind.True;
        var isAllowed = !flagged && !HasAnyModerationFlag(info);

        return new ModerationDecision
        {
            IsAllowed = isAllowed,
            Categories = info
        };
    }

    public async Task<IdeaNudgeDecision> AssessIdeaNudge(IdeaNudgeAssessmentRequest request)
    {
        var payload = new
        {
            model = _completionsModel,
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = BuildNudgingSystemPrompt(request.NudgingMode)
                },
                new
                {
                    role = "user",
                    content = BuildNudgingUserPrompt(request)
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync("chat/completions", payload);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new AiException($"Mistral idea nudging failed ({(int)response.StatusCode}): {body}", null);
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            return new IdeaNudgeDecision { IsApproved = true };
        }

        var content = choices[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            return new IdeaNudgeDecision { IsApproved = true };
        }

        return ParseNudgeDecision(content);
    }

    public async Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null)
    {
        var payload = new
        {
            model = _completionsModel,
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You rewrite unsafe user feedback into respectful, constructive feedback while preserving intent. Return only the rewritten text."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync("chat/completions", payload);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new AiException($"Mistral suggestion generation failed ({(int)response.StatusCode}): {body}", null);
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            return "Please rephrase your message in a respectful way.";
        }

        var content = choices[0].GetProperty("message").GetProperty("content").GetString();
        return string.IsNullOrWhiteSpace(content) ? "Please rephrase your message in a respectful way." : content.Trim();
    }

    public async Task<IEnumerable<int>> RankIdeasByRelation(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
    {
        if (candidateIdeas.Count == 0 || limit <= 0)
        {
            return Array.Empty<int>();
        }

        int cappedLimit = Math.Min(limit, candidateIdeas.Count);
        var prompt = BuildIdeaRankingPrompt(referenceIdea, candidateIdeas, preferDifferent, cappedLimit);
        var payload = new
        {
            model = _completionsModel,
            temperature = 0.1,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You compare youth ideas by meaning. Return only strict JSON with field rankedIndexes as an array of integer indexes. For similarity tasks, return clearly similar ideas. For difference tasks, return ideas with a noticeably different focus or approach; be inclusive rather than restrictive."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync("chat/completions", payload);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new AiException($"Mistral idea ranking failed ({(int)response.StatusCode}): {body}", null);
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            return Array.Empty<int>();
        }

        var content = choices[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<int>();
        }

        return ParseRankedIndexes(content, candidateIdeas.Count, cappedLimit);
    }

    public async Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> CategorizeIdeas(
        IReadOnlyList<string> ideas,
        IReadOnlyList<string> existingCategories,
        int maxCategoriesPerIdea)
    {
        if (ideas.Count == 0)
        {
            return new Dictionary<int, IReadOnlyList<string>>();
        }

        int cappedMax = Math.Clamp(maxCategoriesPerIdea, 1, 4);
        var prompt = BuildIdeaCategorizationPrompt(ideas, existingCategories, cappedMax);
        var payload = new
        {
            model = _completionsModel,
            temperature = 0.1,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You assign semantic categories to youth ideas. Return only strict JSON."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync("chat/completions", payload);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new AiException($"Mistral idea categorization failed ({(int)response.StatusCode}): {body}", null);
        }

        using var document = JsonDocument.Parse(body);
        if (!document.RootElement.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            return new Dictionary<int, IReadOnlyList<string>>();
        }

        var content = choices[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            return new Dictionary<int, IReadOnlyList<string>>();
        }

        return ParseCategorizedIdeas(content, ideas.Count, cappedMax);
    }

    private static string BuildIdeaCategorizationPrompt(IReadOnlyList<string> ideas, IReadOnlyList<string> existingCategories, int maxCategoriesPerIdea)
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

    private static string BuildIdeaRankingPrompt(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
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

    private static string BuildNudgingSystemPrompt(string nudgingMode)
    {
        return $"You help youth improve the quality of their idea before publishing. Ask exactly one concrete follow-up question when the idea is too shallow, vague, or underspecified. If the idea is already acceptable for the configured nudging strength, approve it. Never invent multiple questions. Return strict JSON only with the shape {{\"isApproved\":true}} or {{\"isApproved\":false,\"question\":\"...\"}}. Nudging strength: {DescribeNudgingMode(nudgingMode)}.";
    }

    private static string BuildNudgingUserPrompt(IdeaNudgeAssessmentRequest request)
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

    private static ModerationInfo ParseModerationInfo(JsonElement result)
    {
        if (!result.TryGetProperty("categories", out var categories) || categories.ValueKind != JsonValueKind.Object)
        {
            return new ModerationInfo();
        }

        return new ModerationInfo
        {
            Sexual = GetBoolean(categories, "sexual"),
            HateAndDiscrimination = GetBoolean(categories, "hate_and_discrimination") || GetBoolean(categories, "hate"),
            ViolenceAndThreats = GetBoolean(categories, "violence_and_threats") || GetBoolean(categories, "violence"),
            DangerousAndCriminalContent = GetBoolean(categories, "dangerous_and_criminal_content"),
            SelfHarm = GetBoolean(categories, "self_harm"),
            Pii = GetBoolean(categories, "pii")
        };
    }

    private static bool GetBoolean(JsonElement obj, string property)
    {
        if (!obj.TryGetProperty(property, out var value)) return false;
        return value.ValueKind == JsonValueKind.True;
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

    public async Task<ExtractKeyPhrasesResponse> ExtractKeyPhrases(
        string transcript,
        string language,
        int maxPhrases,
        IReadOnlyList<string> existingPhrases = null,
        IReadOnlyList<string> rejectedPhrases = null)
    {
        if (string.IsNullOrWhiteSpace(transcript) || maxPhrases <= 0)
            return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());

        // Build rejection context for AI learning
        var rejectionContext = BuildRejectionContext(existingPhrases, rejectedPhrases);

        // Build user prompt using StringBuilder to avoid raw string literal issues
        var userPrompt = new StringBuilder();
        userPrompt.AppendLine($"From the following {language} speech transcript, extract EXACTLY up to {maxPhrases} unique, meaningful key phrases.");
        userPrompt.AppendLine();
        
        userPrompt.AppendLine("### CONTEXT:");
        userPrompt.AppendLine($"Existing concepts (do NOT repeat or paraphrase): {JsonSerializer.Serialize(existingPhrases ?? new List<string>())}");
        userPrompt.AppendLine($"Rejected concepts (NEVER suggest, even as synonyms): {JsonSerializer.Serialize(rejectedPhrases ?? new List<string>())}");
        userPrompt.AppendLine();
        
        userPrompt.AppendLine("### STRICT RULES:");
        userPrompt.AppendLine("1. **Role**: Act as a meeting note-taker. Convert spoken words into brief, meaningful notes.");
        userPrompt.AppendLine("2. **Content**: ONLY extract substantive opinions, observations, or actionable points. Skip:");
        userPrompt.AppendLine("   - All greetings: \"hallo\", \"hi\", \"hey\", \"hoi\"");
        userPrompt.AppendLine("   - All filler: \"oke\", \"dus\", \"eigenlijk\", \"ik vind dat\", \"ik bedoel\", \"kijk\", \"ja\", \"nee\", \"wel\", \"even\"");
        userPrompt.AppendLine("   - All conversation starters: \"Wanneer ik...\", \"Als ik...\", \"Wat als...\"");
        userPrompt.AppendLine("   - All acknowledgments: \"keigoed\", \"goed\", \"fijn\", \"mooi\", \"leuk\"");
        userPrompt.AppendLine("   - All speech artifacts: \"eh\", \"hmm\", \"oh\", \"ah\", \"tja\"");
        userPrompt.AppendLine("3. **Length**: Each phrase MUST be 2-5 words (inclusive). No exceptions.");
        userPrompt.AppendLine("4. **Uniqueness**: Remove ALL duplicate or near-duplicate phrases (case-insensitive).");
        userPrompt.AppendLine("5. **Semantic Check**: If a phrase means the same as an existing or rejected concept (even with different wording), SKIP it.");
        userPrompt.AppendLine($"6. **Language**: Maintain the original {language} language in all phrases.");
        userPrompt.AppendLine("7. **New Only**: Only extract concepts NOT already in existing or rejected lists.");
        userPrompt.AppendLine("8. **Format**: Return ONLY a JSON object with format: {\"phrases\": [\"phrase 1\", \"phrase 2\"]}");
        userPrompt.AppendLine("   - No markdown, no explanations, no additional fields");
        userPrompt.AppendLine("   - Empty array if no valid new concepts: {\"phrases\": []}");
        userPrompt.AppendLine();
        userPrompt.AppendLine("### EXAMPLES:");
        userPrompt.AppendLine("Example 1 (Dutch):");
        userPrompt.AppendLine("Transcript: \"Ik vind dat de toegang tot mental health zorg echt verbeterd moet worden. Ook de wachtlijsten zijn veel te lang.\"");
        userPrompt.AppendLine("Output: {\"phrases\": [\"Improve mental health access\", \"Reduce waiting lists\"]}");
        userPrompt.AppendLine();
        userPrompt.AppendLine("Example 2 (Dutch):");
        userPrompt.AppendLine("Transcript: \"Hallo, hoe gaat het? Ik ben het helemaal eens met het vorige punt.\"");
        userPrompt.AppendLine("Output: {\"phrases\": []}");
        userPrompt.AppendLine();
        userPrompt.AppendLine("Example 3 (English):");
        userPrompt.AppendLine("Transcript: \"The system should support real-time collaboration. Users need to see each other's cursors.\"");
        userPrompt.AppendLine("Output: {\"phrases\": [\"Support real-time collaboration\", \"Show user cursors\"]}");
        userPrompt.AppendLine();
        userPrompt.AppendLine("### TRANSCRIPT:");
        userPrompt.Append(transcript);

        var payload = new
        {
            model = _keyPhraseModel,
            temperature = 0.1,
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new {
                    role = "system",
                    content = "You are a professional note-taking assistant. You ALWAYS return valid JSON. " +
                             "Extract concise, meaningful key phrases from spoken language as if taking meeting notes. " +
                             "Be precise, remove all fluff, focus on actionable content, and never include filler words or greetings."
                },
                new {
                    role = "user",
                    content = userPrompt.ToString()
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync("chat/completions", payload);
        var body = await response.Content.ReadAsStringAsync();

        // Handle rate limiting (429 Too Many Requests) with retry logic
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            System.Diagnostics.Debug.WriteLine($"[MagicMode] Mistral API rate limited. Retrying...");
            const int maxRetries = 3;
            
            for (int retry = 1; retry <= maxRetries; retry++)
            {
                var delay = TimeSpan.FromSeconds(retry);
                System.Diagnostics.Debug.WriteLine($"[MagicMode] Retry {retry}/{maxRetries} after {delay.TotalSeconds}s delay");
                await Task.Delay(delay);
                
                var retryResponse = await _httpClient.PostAsJsonAsync("chat/completions", payload);
                var retryBody = await retryResponse.Content.ReadAsStringAsync();
                
                if (retryResponse.IsSuccessStatusCode)
                {
                    body = retryBody;
                    System.Diagnostics.Debug.WriteLine($"[MagicMode] Mistral API response (retry {retry}): {body}");
                    break;
                }
                else if (retry == maxRetries)
                {
                    System.Diagnostics.Debug.WriteLine($"[MagicMode] Mistral API failed after {maxRetries} retries");
                    throw new AiException($"Mistral key phrase extraction failed after retries ({(int)retryResponse.StatusCode}): {retryBody}", null);
                }
            }
        }
        else if (!response.IsSuccessStatusCode)
        {
            throw new AiException($"Mistral key phrase extraction failed ({(int)response.StatusCode}): {body}", null);
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());
        }

        // Extract content - handle both string and direct JSON object responses
        JsonElement contentElement;
        try
        {
            var message = choices[0].GetProperty("message");
            contentElement = message.GetProperty("content");
        }
        catch (Exception)
        {
            return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());
        }

        // Handle JSON mode response (may already be parsed as object with phrases array)
        if (contentElement.ValueKind == JsonValueKind.Object &&
            contentElement.TryGetProperty("phrases", out var phrasesArray) &&
            phrasesArray.ValueKind == JsonValueKind.Array)
        {
            var phrases = ParseAndCleanPhrases(phrasesArray, existingPhrases, rejectedPhrases, maxPhrases, out var rejected);
            return new ExtractKeyPhrasesResponse(phrases, rejected);
        }

        // Handle string response (fallback for non-JSON mode or older Mistral versions)
        var contentString = contentElement.GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(contentString))
        {
            return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());
        }

        // Strip markdown code fences if present
        var raw = contentString.Trim();
        if (raw.StartsWith("```json") || raw.StartsWith("```"))
        {
            var lines = raw.Split('\n');
            raw = string.Join("\n", lines.Skip(1).Take(lines.Length - 2)).Trim();
        }

        // Try to parse as JSON with phrases array
        try
        {
            using var innerDoc = JsonDocument.Parse(raw);
            if (innerDoc.RootElement.ValueKind == JsonValueKind.Object &&
                innerDoc.RootElement.TryGetProperty("phrases", out var phrasesProp) &&
                phrasesProp.ValueKind == JsonValueKind.Array)
            {
                var phrases = ParseAndCleanPhrases(phrasesProp, existingPhrases, rejectedPhrases, maxPhrases, out var rejected);
                return new ExtractKeyPhrasesResponse(phrases, rejected);
            }
            // Direct array format fallback
            if (innerDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var phrases = ParseAndCleanPhrases(innerDoc.RootElement, existingPhrases, rejectedPhrases, maxPhrases, out var rejected);
                return new ExtractKeyPhrasesResponse(phrases, rejected);
            }
        }
        catch (JsonException)
        {
            return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());
        }

        return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());
    }

    private IReadOnlyList<string> ParseAndCleanPhrases(
        JsonElement phrasesArray,
        IReadOnlyList<string> existingPhrases,
        IReadOnlyList<string> rejectedPhrases,
        int maxPhrases,
        out IReadOnlyList<RejectedPhrase> rejectedPhrasesWithReasons)
    {
        var phrases = new List<string>();
        var rejectedList = new List<RejectedPhrase>();

        foreach (var element in phrasesArray.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                var phrase = element.GetString()?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(phrase))
                {
                    phrases.Add(phrase);
                }
            }
        }

        // Post-processing for quality and constraints
        var cleaned = new List<string>();
        var existingPhrasesSet = new HashSet<string>(existingPhrases ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
        var rejectedPhrasesSet = new HashSet<string>(rejectedPhrases ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

        foreach (var phrase in phrases)
        {
            var wordCount = phrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            // Check word count
            if (wordCount < 2)
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.WordCountTooLow));
                continue;
            }
            if (wordCount > 5)
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.WordCountExceeded));
                continue;
            }

            // Check exact duplicates
            if (existingPhrasesSet.Contains(phrase) || rejectedPhrasesSet.Contains(phrase))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.DuplicateExact));
                continue;
            }

            // Check subset of existing (e.g., "spoor" when "spoornetwerk" exists)
            var isSubset = false;
            var similarTo = "";
            foreach (var existing in existingPhrasesSet)
            {
                if (IsSubset(phrase, existing))
                {
                    isSubset = true;
                    similarTo = existing;
                    break;
                }
                if (IsSubset(existing, phrase))
                {
                    // The existing phrase is a subset of the new one, so reject the new one
                    isSubset = true;
                    similarTo = existing;
                    break;
                }
            }
            if (isSubset)
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.SubsetOfExisting, similarTo));
                continue;
            }

            // Check semantic duplicates using Jaccard similarity
            var isSemanticDuplicate = false;
            foreach (var existing in existingPhrasesSet)
            {
                if (JaccardSimilarity(phrase, existing) > 0.6)
                {
                    isSemanticDuplicate = true;
                    similarTo = existing;
                    break;
                }
            }
            if (isSemanticDuplicate)
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.DuplicateSemantic, similarTo));
                continue;
            }

            // Check for filler content
            if (ContainsFillerWords(phrase))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.FillerContent));
                continue;
            }

            // Check if too generic
            if (IsTooGeneric(phrase))
            {
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.TooGeneric));
                continue;
            }

            cleaned.Add(phrase);
        }

        // Apply stemming-based deduplication on cleaned list
        cleaned = ApplyStemmingDeduplication(cleaned, rejectedList);

        // Limit to maxPhrases
        var finalPhrases = cleaned.Take(maxPhrases).ToList();
        
        // Add phrases that were cut due to maxPhrases limit
        foreach (var phrase in cleaned.Skip(maxPhrases))
        {
            rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.TooGeneric, "Exceeded max phrases limit"));
        }

        rejectedPhrasesWithReasons = rejectedList.AsReadOnly();
        return finalPhrases.AsReadOnly();
    }

    private bool IsSubset(string phraseA, string phraseB)
    {
        var wordsA = phraseA.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var wordsB = phraseB.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (wordsA.Length >= wordsB.Length) return false;
        
        var setB = new HashSet<string>(wordsB, StringComparer.OrdinalIgnoreCase);
        return wordsA.All(w => setB.Contains(w));
    }

    private double JaccardSimilarity(string phraseA, string phraseB)
    {
        var wordsA = phraseA.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var wordsB = phraseB.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        var setA = new HashSet<string>(wordsA, StringComparer.OrdinalIgnoreCase);
        var setB = new HashSet<string>(wordsB, StringComparer.OrdinalIgnoreCase);
        
        var intersection = setA.Count(w => setB.Contains(w));
        var union = setA.Count + setB.Count - intersection;
        
        return union == 0 ? 0 : (double)intersection / union;
    }

    private bool ContainsFillerWords(string phrase)
    {
        var fillerWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "enkele", "enkelei", "enkele", "enige", "sommige", "verschillende",
            "hier", "daar", "dit", "dat", "deze", "die", "het", "een",
            "van", "in", "op", "te", "voor", "met", "door", "bij", "uit",
            "als", "dat", "wat", "die", "die", "welke", "waar",
            "is", "zijn", "was", "waren", "wordt", "worden",
            "heeft", "hebben", "had", "hadden",
            "zeer", "erg", "heel", "veel", "meeste"
        };
        
        var words = phrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        // If more than 30% of words are fillers, reject
        if (words.Length == 0) return false;
        var fillerCount = words.Count(w => fillerWords.Contains(w));
        return (double)fillerCount / words.Length > 0.3;
    }

    private bool IsTooGeneric(string phrase)
    {
        var genericPhrases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "enige voorbeelden",
            "verschillende dingen",
            "diverse zaken",
            "veel dingen",
            "dergelijke",
            "en dergelijke",
            "etc",
            "enzovoort",
            "enzovoorts"
        };
        
        return genericPhrases.Contains(phrase.ToLower());
    }

    private List<string> ApplyStemmingDeduplication(List<string> phrases, List<RejectedPhrase> rejectedList)
    {
        var result = new List<string>();
        var seenStems = new HashSet<string>();
        
        foreach (var phrase in phrases)
        {
            var stem = StemPhrase(phrase);
            if (seenStems.Contains(stem))
            {
                // Find which phrase this is a stem duplicate of
                var similarTo = result.FirstOrDefault(p => StemPhrase(p) == stem);
                rejectedList.Add(new RejectedPhrase(phrase, PhraseRejectionReason.DuplicateSemantic, similarTo));
            }
            else
            {
                seenStems.Add(stem);
                result.Add(phrase);
            }
        }
        
        return result;
    }

    private string StemPhrase(string phrase)
    {
        var words = phrase.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var stemmedWords = words.Select(StemWord).ToArray();
        return string.Join(" ", stemmedWords);
    }

    private string StemWord(string word)
    {
        var lowerWord = word.ToLower();
        
        // Dutch stemming rules (simplified)
        // Remove common prefixes/suffixes for Dutch
        if (lowerWord.EndsWith("ing") && lowerWord.Length > 4)
            return lowerWord.Substring(0, lowerWord.Length - 3);
        if (lowerWord.EndsWith("en") && lowerWord.Length > 3)
            return lowerWord.Substring(0, lowerWord.Length - 2);
        if (lowerWord.EndsWith("s") && lowerWord.Length > 3)
            return lowerWord.Substring(0, lowerWord.Length - 1);
        if (lowerWord.EndsWith("te") && lowerWord.Length > 4)
            return lowerWord.Substring(0, lowerWord.Length - 2);
        if (lowerWord.EndsWith("de") && lowerWord.Length > 4)
            return lowerWord.Substring(0, lowerWord.Length - 2);
        if (lowerWord.EndsWith("heid") && lowerWord.Length > 5)
            return lowerWord.Substring(0, lowerWord.Length - 4);
        if (lowerWord.EndsWith("atie") && lowerWord.Length > 5)
            return lowerWord.Substring(0, lowerWord.Length - 3);
        if (lowerWord.EndsWith("tion") && lowerWord.Length > 5)
            return lowerWord.Substring(0, lowerWord.Length - 4);
        
        return lowerWord;
    }

    private string BuildRejectionContext(IReadOnlyList<string> existingPhrases, IReadOnlyList<string> rejectedPhrases)
    {
        // For now, we don't have session-level tracking of rejected phrases with reasons
        // This can be extended in the future by maintaining a session state
        // For example, you could pass rejectedPhrasesWithReasons from the client
        // and format them here for the AI to learn from
        
        // If you want to implement this, you would:
        // 1. Track rejected phrases with reasons in the session/client
        // 2. Pass them to the API
        // 3. Format them here like:
        //    "- [REJECTED: word_count_exceeded] 'some long phrase here'"
        
        return string.Empty;
    }
}
