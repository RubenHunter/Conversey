using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ai.Speech;

namespace Conversey.BL.Ai.Speech;

public class OpenAiSpeechManager : ISpeechManager
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiSpeechManager> _logger;
    private readonly string _sttModel;
    private readonly string _ttsModel;
    private readonly string _ttsVoice;

    private static readonly Dictionary<Language, string> VoiceMap = new()
    {
        { Language.en, "nova" },
        { Language.nl, "nova" },
        { Language.fr, "nova" },
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

    public async Task<string> TranscribeSpeechAsync(Stream audioStream, Language language, IEnumerable<string> contextBias = null, AudioMimeType mimeType = null)
    {
        mimeType ??= AudioMimeType.Webm;
        try
        {
            using var content = new MultipartFormDataContent();

            var audioMemoryStream = new MemoryStream();
            await audioStream.CopyToAsync(audioMemoryStream);
            var audioBytes = audioMemoryStream.ToArray();
            var audioContent = new ByteArrayContent(audioBytes);
            
            audioContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType.Value);
            content.Add(audioContent, "file", $"audio.{mimeType.FileExtension}");

            content.Add(new StringContent(_sttModel), "model");
            content.Add(new StringContent(language.ToString()), "language");

            var request = new HttpRequestMessage(HttpMethod.Post, "audio/transcriptions");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI STT HTTP {Status}: {Body}", (int)response.StatusCode, responseContent);
                throw new HttpRequestException($"OpenAI STT returned {response.StatusCode}: {responseContent}");
            }

            try
            {
                var result = JsonSerializer.Deserialize<OpenAiTranscriptionResponse>(responseContent);
                return result?.Text ?? string.Empty;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "OpenAI STT JSON parse failed. Raw response: {Body}", responseContent);
                throw;
            }
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

    public async Task<Stream> SynthesizeSpeechAsync(string text, Language language)
    {
        try
        {
            var voice = VoiceMap.TryGetValue(language, out var v) ? v : "nova";

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

    private class OpenAiTranscriptionResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}