using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Conversey.BL.Ai;

public class MistralAiManager : IAiManager
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelName;
    private readonly string _moderationModel;

    public MistralAiManager(HttpClient httpClient, string apiKey, string modelName = "mistral-small-latest", string moderationModel = "mistral-moderation-latest")
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _modelName = modelName;
        _moderationModel = moderationModel;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }
    
    public string GenerateAiAlternative(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

        Console.WriteLine($"Prompt: {prompt}");

        var request = new
        {
            model = _modelName,
            temperature = 0.2,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You rewrite offensive or toxic user text into a respectful and safe alternative for a youth platform. Preserve the original meaning as much as possible, but remove insults, hate, threats, sexual harassment, and other inappropriate language. Return only the rewritten alternative text and nothing else."
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
        
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = _httpClient.PostAsync("chat/completions", content).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;
            
            Console.WriteLine($"Alternative response body: {responseJson}");

            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            
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

    public ModerationDecision ModerateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Inhoud mag niet leeg zijn.", nameof(content));

        var request = new
        {
            model = _moderationModel,
            input = new[] { content }
        };

        var json = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = _httpClient.PostAsync("moderations", httpContent).Result;
            var responseJson = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine($"Response body: {responseJson}");

            response.EnsureSuccessStatusCode();

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
}