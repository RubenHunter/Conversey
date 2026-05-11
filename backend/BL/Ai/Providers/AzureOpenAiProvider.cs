using System.Net.Http.Json;
using System.Text.Json;

namespace Conversey.BL.Ai;

public sealed class AzureOpenAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _deploymentName;
    private readonly string _apiVersion;
    public string ProviderName => "Azure";

    public bool SupportsNativeModeration => false;

    public AzureOpenAiProvider(HttpClient httpClient, string deploymentName, string apiVersion)
    {
        _httpClient = httpClient;
        _deploymentName = deploymentName;
        _apiVersion = string.IsNullOrWhiteSpace(apiVersion) ? "2024-10-21" : apiVersion;
    }

    public async Task<AiCompletionResult> CompleteAsync(string systemPrompt, string userPrompt, string model, decimal temperature, CancellationToken cancellationToken = default)
    {
        var deployment = string.IsNullOrWhiteSpace(model) ? _deploymentName : model;
        var payload = new
        {
            temperature,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        var url = $"openai/deployments/{deployment}/chat/completions?api-version={_apiVersion}";
        using var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new AiException($"Azure completions failed ({(int)response.StatusCode}): {body}", null);
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

    public Task<AiModerationResult> ModerateAsync(string content, string model, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AiModerationResult { Flagged = false });
    }

    public async Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var url = $"openai/models?api-version={_apiVersion}";
        using var response = await _httpClient.GetAsync(url, cancellationToken);
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
