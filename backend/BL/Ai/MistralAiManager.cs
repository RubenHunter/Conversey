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
    
    // tijdelijk
    public string GetApiKey()
    {
        return string.IsNullOrWhiteSpace(_apiKey) ? "*****" : "leeg";
    }

    public string GetModelName()
    {
        return _modelName;
    }
    // —————————————


    public async Task<string> GenerateResponseAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

        Console.WriteLine($"Prompt: {prompt}");

        var request = new
        {
            model = _modelName,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };
        
        var json = JsonSerializer.Serialize(request);
        Console.WriteLine($"Verzonden request: {json}");
        
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response status: {response.StatusCode}"); // Log de statuscode
            Console.WriteLine($"Response body: {responseJson}"); // Log de response body

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Mistral API returned status code: {response.StatusCode}. Response: {responseJson}");
            }
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            
            var result = JsonSerializer.Deserialize<Response>(responseJson, options);

            if (result?.Choices == null || result.Choices.Length == 0)
            {
                throw new Exception($"Ongeldige response van Mistral API: Geen 'choices' gevonden. Response: {responseJson}");
            }

            Console.WriteLine($"AI response content: {result.Choices[0].Message.Content}");

            return result.Choices[0].Message.Content;
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

    public async Task<bool> IsContentAllowedAsync(string content)
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
        var response = await _httpClient.PostAsync("chat/moderations", httpContent);
        var responseJson = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Response status: {response.StatusCode}");
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

        // Eenvoudige policy:
        // blokkeer als één van deze categorieën flagged is
        var blocked =
            moderation.Categories.HateAndDiscrimination ||
            moderation.Categories.ViolenceAndThreats ||
            moderation.Categories.DangerousAndCriminalContent ||
            moderation.Categories.SelfHarm;

        return !blocked;
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