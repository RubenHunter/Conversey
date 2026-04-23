using System.Net.Http.Json;
using System.Text.Json;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Ideation;
using Microsoft.Extensions.AI;

namespace Conversey.BL.Ai;

public sealed class MistralAiManager : IAiManager
{
    private readonly HttpClient _httpClient;
    private readonly string _completionsModel;
    private readonly string _moderationModel;

    public MistralAiManager(HttpClient httpClient, AiManagerConfig config)
    {
        _httpClient = httpClient;
        _completionsModel = string.IsNullOrWhiteSpace(config.CompletionsModel) ? "mistral-small-latest" : config.CompletionsModel;
        _moderationModel = string.IsNullOrWhiteSpace(config.ModerationModel) ? "mistral-moderation-latest" : config.ModerationModel;
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

    public async Task<IReadOnlyList<int>> RankIdeasByRelation(string referenceIdea, IReadOnlyList<string> candidateIdeas, bool preferDifferent, int limit)
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
                if (!item.TryGetProperty("index", out var indexElement) || !indexElement.TryGetInt32(out int index)) continue;
                if (index < 0 || index >= ideaCount) continue;

                if (!item.TryGetProperty("categories", out var categoriesElement) || categoriesElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var categories = categoriesElement
                    .EnumerateArray()
                    .Where(element => element.ValueKind == JsonValueKind.String)
                    .Select(element => element.GetString()?.Trim() ?? string.Empty)
                    .Where(category => category.Length > 0)
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
        catch (JsonException)
        {
            return Array.Empty<int>();
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
}


