using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Conversey.BL.Speech;

public class MistralSpeechManager : IMistralSpeechManager
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _sttModel;
    private readonly string _ttsModel;
    private readonly IMistralVoiceManager _voiceManager;

    public MistralSpeechManager(
        HttpClient httpClient,
        string apiKey,
        IMistralVoiceManager voiceManager,
        string sttModel = "voxtral-mini-latest",
        string ttsModel = "voxtral-mini-tts-latest")
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _voiceManager = voiceManager;
        _sttModel = sttModel;
        _ttsModel = ttsModel;
    }

    public async Task<string> TranscribeSpeechAsync(Stream audioStream, string language, IEnumerable<string>? contextBias = null)
    {
        try
        {
            using var content = new MultipartFormDataContent();

            using var audioMemoryStream = new MemoryStream();
            await audioStream.CopyToAsync(audioMemoryStream);
            var audioBytes = audioMemoryStream.ToArray();
            var audioContent = new ByteArrayContent(audioBytes);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");
            content.Add(audioContent, "file", "audio.webm");

            content.Add(new StringContent(_sttModel), "model");
            content.Add(new StringContent(language), "language");

            var biasList = contextBias?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (biasList?.Count > 0)
            {
                content.Add(new StringContent(JsonSerializer.Serialize(biasList)), "context_bias");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "audio/transcriptions");
            request.Content = content;

            // Debug: Log audio size
            Console.WriteLine($"[DEBUG] Audio size: {audioBytes.Length} bytes, Model: {_sttModel}, Language: {language}");

            var response = await _httpClient.SendAsync(request);
            
            // Debug logging
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Mistral STT Response: {response.StatusCode}");
            Console.WriteLine($"[DEBUG] Response Body: {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Mistral API returned: {response.StatusCode}, Response: {responseContent}");
            }

            // Parse the JSON response (Mistral returns JSON even for multipart requests)
            var result = JsonSerializer.Deserialize<MistralTranscriptionResponse>(responseContent);
            
            return result?.Text ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Mistral STT API error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error transcribing speech: {ex.Message}", ex);
        }
    }

    public async Task<Stream> SynthesizeSpeechAsync(string text, string language)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "audio/speech");

            var voice = _voiceManager.GetVoiceForLanguage(language);
            var payload = new
            {
                model = _ttsModel,
                input = text,
                voice,
                response_format = "mp3"
            };
            
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
            
            request.Content = jsonContent;

            var response = await _httpClient.SendAsync(request);
            
            // Debug logging
            var responseContentText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Mistral TTS Response: {response.StatusCode}, Body length: {responseContentText.Length}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Mistral TTS API returned: {response.StatusCode}, Response: {responseContentText}");
            }

            // Mistral returns JSON with base64 audio_data, not a direct stream
            var result = JsonSerializer.Deserialize<MistralTtsResponse>(responseContentText);
            if (string.IsNullOrEmpty(result?.AudioData))
            {
                throw new HttpRequestException("No audio_data in response from Mistral TTS API");
            }
            
            // Convert base64 to byte array and return as stream
            var audioBytes = Convert.FromBase64String(result.AudioData);
            return new MemoryStream(audioBytes);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Mistral TTS API error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error synthesizing speech: {ex.Message}", ex);
        }
    }

    private class MistralTranscriptionResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class MistralTtsResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("audio_data")]
        public string AudioData { get; set; } = string.Empty;
    }
}
