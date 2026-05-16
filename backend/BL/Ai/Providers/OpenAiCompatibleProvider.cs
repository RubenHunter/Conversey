using System.Net.Http.Json;
using System.Text.Json;

namespace Conversey.BL.Ai;

public sealed class OpenAiCompatibleProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    public string ProviderName { get; }

    public bool SupportsNativeModeration => false;

    public OpenAiCompatibleProvider(HttpClient httpClient, string providerName)
    {
        _httpClient = httpClient;
        ProviderName = providerName;
    }

    public async Task<AiCompletionResult> CompleteAsync(string systemPrompt, string userPrompt, string model, decimal temperature, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model,
            temperature,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync("chat/completions", payload, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new AiException($"{ProviderName} completions failed ({(int)response.StatusCode}): {body}", null);
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            return new AiCompletionResult { Content = string.Empty };
        }

        var content = choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        var usage = root.TryGetProperty("usage", out var usageElement) ? usageElement : default;

        return new AiCompletionResult
        {
            Content = content.Trim(),
            PromptTokens = usage.ValueKind == JsonValueKind.Object && usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : 0,
            CompletionTokens = usage.ValueKind == JsonValueKind.Object && usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt32() : 0
        };
    }

    public async Task<AiModerationResult> ModerateAsync(string content, string model, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = string.IsNullOrWhiteSpace(model) ? "omit" : model,
            input = content
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("moderations", payload, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new AiModerationResult { Flagged = false };
            }

            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (!root.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array || results.GetArrayLength() == 0)
            {
                return new AiModerationResult { Flagged = false };
            }

            var first = results[0];
            var flagged = first.TryGetProperty("flagged", out var flaggedElement) && flaggedElement.ValueKind == JsonValueKind.True;

            var categories = new Dictionary<string, bool>();
            if (first.TryGetProperty("categories", out var cats) && cats.ValueKind == JsonValueKind.Object)
            {
                foreach (var cat in cats.EnumerateObject())
                {
                    categories[cat.Name] = cat.Value.ValueKind == JsonValueKind.True;
                }
            }

            return new AiModerationResult { Flagged = flagged, Categories = categories };
        }
        catch (HttpRequestException)
        {
            return new AiModerationResult { Flagged = false };
        }
    }

    public async Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync("models", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<string>();
        }

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<string>();
        }

        return data.EnumerateArray()
            .Where(e => e.TryGetProperty("id", out _))
            .Select(e => e.GetProperty("id").GetString())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToList()
            .AsReadOnly();
    }
}
