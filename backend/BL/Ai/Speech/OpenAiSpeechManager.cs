using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace Conversey.BL.Ai.Speech;

public class OpenAiSpeechManager : ISpeechManager
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiSpeechManager> _logger;
    private readonly string _sttModel;
    private readonly string _ttsModel;
    private readonly string _ttsVoice;

    private static readonly Dictionary<string, string> VoiceMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "en", "nova" },
        { "nl", "nova" },
        { "fr", "nova" },
    };

    public OpenAiSpeechManager(
        HttpClient httpClient,
        ILogger<OpenAiSpeechManager> logger,
        string sttModel = "whisper-1",
        string ttsModel = "tts-1",
        string ttsVoice = "nova")
    {
        _httpClient = httpClient;
        _logger = logger;
        _sttModel = sttModel;
        _ttsModel = ttsModel;
        _ttsVoice = ttsVoice;
        _logger.LogInformation("[Speech:OpenAI] STT={SttModel} TTS={TtsModel} Voice={Voice} BaseUrl={BaseUrl}",
            _sttModel, _ttsModel, _ttsVoice, httpClient.BaseAddress);
    }

    public async Task<string> TranscribeSpeechAsync(Stream audioStream, string language, IEnumerable<string> contextBias = null, string mimeType = "audio/webm")
    {
        try
        {
            using var content = new MultipartFormDataContent();

            var audioMemoryStream = new MemoryStream();
            await audioStream.CopyToAsync(audioMemoryStream);
            var audioBytes = audioMemoryStream.ToArray();
            var audioContent = new ByteArrayContent(audioBytes);
            var baseContentType = mimeType.Split(';')[0].Trim();
            var fileExtension = GetAudioExtension(baseContentType);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue(baseContentType);
            content.Add(audioContent, "file", $"audio.{fileExtension}");

            content.Add(new StringContent(_sttModel), "model");
            content.Add(new StringContent(language), "language");
            content.Add(new StringContent("text"), "response_format");

            var request = new HttpRequestMessage(HttpMethod.Post, "audio/transcriptions");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"OpenAI STT returned {response.StatusCode}: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<OpenAiTranscriptionResponse>(responseContent);
            return result?.Text ?? string.Empty;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"OpenAI STT error: {ex.Message}", ex);
        }
    }

    public async Task<Stream> SynthesizeSpeechAsync(string text, string language)
    {
        try
        {
            var voice = VoiceMap.TryGetValue(language ?? "en", out var v) ? v : "nova";

            var payload = new
            {
                model = _ttsModel,
                input = text,
                voice,
                response_format = "mp3"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "audio/speech");
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"OpenAI TTS returned {response.StatusCode}: {errorText}");
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync();
            return new MemoryStream(audioBytes);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"OpenAI TTS error: {ex.Message}", ex);
        }
    }

    private static string GetAudioExtension(string baseContentType) => baseContentType switch
    {
        "audio/mp4" or "audio/m4a" or "audio/mpeg" or "audio/mpga" => "mp4",
        "audio/wav" or "audio/wave" or "audio/x-wav" => "wav",
        "audio/ogg" => "ogg",
        "audio/mp3" => "mp3",
        _ => "webm"
    };

    private class OpenAiTranscriptionResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
