using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Conversey.BL.Ai;

public class MistralAiManager : IAiManager
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _completionsModel;
    private readonly string _moderationModel;
    
    public AiModel CurrentModel { get; private set; }

    public MistralAiManager([FromKeyedServices("MistralAPI")]HttpClient httpClient, AiManagerConfig config)
    {
        _httpClient = httpClient;
        _apiKey = config.ApiKey ?? string.Empty;
        _completionsModel = config.CompletionsModel ?? "mistral-small-latest";
        _moderationModel = config.ModerationModel ?? "mistral-moderation-latest";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);     
        
        CurrentModel = new AiModel
        {
            Name = _completionsModel,
            Type = ModelType.Completions
        };
    }
    
    public void SetCurrentModel(string modelName, ModelType modelType)
    {
        CurrentModel = new AiModel
        {
            Name = modelName,
            Type = modelType
        };
    }
    
    public async Task<string> GenerateAiAlternative(string prompt, ModerationDecision decision = null)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));
        
        SetCurrentModel(_completionsModel, ModelType.Completions);
        
        Console.WriteLine($"Prompt: {prompt}");

        var request = new
        {
            model = CurrentModel.Name,
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You are an assistant that rewrites offensive or toxic user text into a respectful and safe alternative for a youth platform. " +
                    "Your task is to preserve the original meaning as much as possible, but remove insults, hate speech, threats, sexual harassment, " +
                    "and other inappropriate language. Return only the rewritten alternative text and nothing else." +
                    (decision != null ? $"\n\nThe original text contains the following issues:\n" +
                    $"- Hate speech: {decision.Categories.HateAndDiscrimination}\n" +
                    $"- Violence/threats: {decision.Categories.ViolenceAndThreats}\n" +
                    $"- Sexual content: {decision.Categories.Sexual}\n" +
                    $"- Dangerous/criminal content: {decision.Categories.DangerousAndCriminalContent}\n" +
                    $"- Self-harm: {decision.Categories.SelfHarm}\n" +
                    $"- Personally Identifiable Information (PII): {decision.Categories.Pii}\n" +
                    "Focus on addressing these specific issues in your rewrite." : "")
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };
        
        var json = JsonSerializer.Serialize(request);
        Console.WriteLine($"Verzonden alternative request: {json}");
        
        var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content);
            
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Alternative response body: {responseJson}");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<Response>(responseJson, options);

            if (result?.Choices == null || result.Choices.Length == 0)
            {
                throw new Exception($"Ongeldige response van Mistral API: Geen 'choices' gevonden. Response: {responseJson}");
            }

            var alternativeText = result.Choices[0].Message.Content?.Trim();

            if (string.IsNullOrWhiteSpace(alternativeText))
            {
                throw new Exception("Mistral API gaf geen alternatieve tekst terug.");
            }

            return alternativeText;

        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw new Exception($"Fout bij het aanroepen van Mistral API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JsonException: {ex.Message}");
            throw new Exception($"Fout bij het deserialiseren van de API-response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw new Exception($"Onverwachte fout: {ex.Message}", ex);
        }
    }

    public async Task<ModerationDecision> ModerateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Inhoud mag niet leeg zijn.", nameof(content));
        
        SetCurrentModel(_moderationModel, ModelType.Moderation);

        var request = new
        {
            model = CurrentModel.Name,
            input = new[] { content }
        };

        var json = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("moderations", httpContent);

            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response body: {responseJson}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<ModerationResponse>(responseJson, options);

            if (result?.Results == null || result.Results.Length == 0)
                throw new Exception("Ongeldige moderation response: geen results gevonden.");

            var moderation = result.Results[0];
            var categories = moderation.Categories;

            Console.WriteLine($"[Mistral] Category flags — sexual={categories.Sexual}, hate={categories.HateAndDiscrimination}, violence={categories.ViolenceAndThreats}, dangerous={categories.DangerousAndCriminalContent}, selfharm={categories.SelfHarm}, pii={categories.Pii}");

            var blocked =
                categories.Sexual ||
                categories.HateAndDiscrimination ||
                categories.ViolenceAndThreats ||
                categories.DangerousAndCriminalContent ||
                categories.SelfHarm ||
                categories.Pii;

            return new ModerationDecision
            {
                IsAllowed = !blocked,
                Categories = categories
            };
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HttpRequestException: {ex.Message}");
            throw new Exception($"Fout bij het aanroepen van Mistral API: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JsonException: {ex.Message}");
            throw new Exception($"Fout bij het deserialiseren van de API-response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public object GetService(Type serviceType, object serviceKey = null)
    {
        throw new NotImplementedException();
    }
}