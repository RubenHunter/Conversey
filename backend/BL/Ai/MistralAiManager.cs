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


