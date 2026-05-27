using System.Text.Json;
using Conversey.BL.Ai.Dto;
using Conversey.BL.Domain.Ai;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ai;

internal static class AiResponseParser
{
    private static readonly HashSet<string> ModelSafetyIndicators = new(StringComparer.OrdinalIgnoreCase)
    {
        "unsafe", "not safe", "hate", "toxic", "violent", "harassment",
        "inappropriate", "offensive", "flag", "harmful", "abuse", "dangerous"
    };

    internal static ModerationDecision ParseModerationPromptResponse(string rawContent)
    {
        string json = rawContent.Trim();

        int fenceStart = json.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (fenceStart >= 0)
        {
            int fenceEnd = json.IndexOf("```", fenceStart + 7);
            if (fenceEnd > fenceStart)
                json = json.Substring(fenceStart + 7, fenceEnd - fenceStart - 7).Trim();
        }
        else if (json.StartsWith("```"))
        {
            int fenceEnd = json.IndexOf("```", 3);
            if (fenceEnd > 0)
                json = json.Substring(3, fenceEnd - 3).Trim();
        }

        json = ExtractJsonObject(json);

        try
        {
            using var parsed = JsonDocument.Parse(json);
            var root = parsed.RootElement;

            var flagged = root.TryGetProperty("flagged", out var flaggedElement) && flaggedElement.ValueKind == JsonValueKind.True;

            var categories = new Dictionary<string, bool>();
            if (root.TryGetProperty("categories", out var cats) && cats.ValueKind == JsonValueKind.Object)
            {
                foreach (var cat in cats.EnumerateObject())
                    categories[cat.Name] = cat.Value.ValueKind == JsonValueKind.True;
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
                return new ModerationDecision { IsAllowed = false };

            return new ModerationDecision { IsAllowed = true };
        }
    }

    internal static IdeaNudgeDecision ParseNudgeDecision(string rawContent)
    {
        string json = ExtractJsonObject(rawContent.Trim());

        try
        {
            using var parsed = JsonDocument.Parse(json);
            var root = parsed.RootElement;
            var decision = new IdeaNudgeDecision
            {
                IsApproved = GetBoolean(root, "isApproved") || GetBoolean(root, "approved")
            };

            if (!decision.IsApproved && root.TryGetProperty("question", out var questionElement) && questionElement.ValueKind == JsonValueKind.String)
                decision.Question = questionElement.GetString()?.Trim();

            if (string.IsNullOrWhiteSpace(decision.Question) && !decision.IsApproved)
                decision.Question = "Can you make this idea more specific for this topic?";

            if (decision.IsApproved)
                decision.Question = null;

            return decision;
        }
        catch (JsonException ex)
        {
            throw new AiException("Invalid AI response for idea nudging", ex);
        }
    }

    internal static IReadOnlyList<int> ParseRankedIndexes(string rawContent, int candidateCount, int limit)
    {
        string json = ExtractJsonObject(rawContent.Trim());

        try
        {
            using var parsed = JsonDocument.Parse(json);
            if (!parsed.RootElement.TryGetProperty("rankedIndexes", out var indexesElement) || indexesElement.ValueKind != JsonValueKind.Array)
                return Array.Empty<int>();

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

    internal static IReadOnlyDictionary<int, IReadOnlyList<string>> ParseCategorizedIdeas(string rawContent, int ideaCount, int maxCategoriesPerIdea)
    {
        var result = new Dictionary<int, IReadOnlyList<string>>();

        string json = ExtractJsonObject(rawContent.Trim());

        try
        {
            using var parsed = JsonDocument.Parse(json);
            if (!parsed.RootElement.TryGetProperty("items", out var itemsElement) || itemsElement.ValueKind != JsonValueKind.Array)
                return result;

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
                    continue;

                var categories = categoriesElement
                    .EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString()?.Trim() ?? string.Empty)
                    .Where(c => c.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(maxCategoriesPerIdea)
                    .ToList();

                if (categories.Count > 0)
                    result[index] = categories.AsReadOnly();
            }

            return result;
        }
        catch (JsonException)
        {
            return result;
        }
    }

    internal static ExtractKeyPhrasesResponse ParseKeyPhrasesResponse(
        string rawContent,
        IReadOnlyList<string> existingPhrases,
        IReadOnlyList<string> rejectedPhrases,
        int maxPhrases)
    {
        var json = rawContent.Trim();

        var fenceStart = json.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (fenceStart >= 0)
        {
            var fenceEnd = json.IndexOf("```", fenceStart + 7);
            if (fenceEnd > fenceStart)
                json = json.Substring(fenceStart + 7, fenceEnd - fenceStart - 7).Trim();
        }
        else if (json.StartsWith("```"))
        {
            var fenceEnd = json.IndexOf("```", 3);
            if (fenceEnd > 0)
                json = json.Substring(3, fenceEnd - 3).Trim();
        }

        try
        {
            using var document = JsonDocument.Parse(json);

            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("phrases", out var phrasesProp) &&
                phrasesProp.ValueKind == JsonValueKind.Array)
            {
                var phrases = KeyPhraseProcessor.ParseAndClean(phrasesProp, existingPhrases, rejectedPhrases, maxPhrases, out var rejected);
                return new ExtractKeyPhrasesResponse(phrases, rejected);
            }

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                var phrases = KeyPhraseProcessor.ParseAndClean(document.RootElement, existingPhrases, rejectedPhrases, maxPhrases, out var rejected);
                return new ExtractKeyPhrasesResponse(phrases, rejected);
            }
        }
        catch (JsonException)
        {
        }

        return new ExtractKeyPhrasesResponse(Array.Empty<string>(), Array.Empty<RejectedPhrase>());
    }

    internal static ModerationInfo ParseModerationInfo(IReadOnlyDictionary<string, bool> categories)
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

    internal static bool HasAnyModerationFlag(ModerationInfo info)
    {
        return info.Sexual ||
               info.HateAndDiscrimination ||
               info.ViolenceAndThreats ||
               info.DangerousAndCriminalContent ||
               info.SelfHarm ||
               info.Pii;
    }

    private static bool GetBoolean(JsonElement obj, string property)
    {
        if (!obj.TryGetProperty(property, out var value)) return false;
        return value.ValueKind == JsonValueKind.True;
    }

    private static string ExtractJsonObject(string json)
    {
        int firstBrace = json.IndexOf('{');
        int lastBrace = json.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
            json = json.Substring(firstBrace, lastBrace - firstBrace + 1);
        return json;
    }
}
