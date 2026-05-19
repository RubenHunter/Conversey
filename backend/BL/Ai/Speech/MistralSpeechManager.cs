using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ai.Speech;

namespace Conversey.BL.Ai.Speech;

public class MistralSpeechManager : ISpeechManager
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MistralSpeechManager> _logger;
    private readonly string _sttModel;
    private readonly string _ttsModel;
    private readonly IVoiceManager _voiceManager;

    public MistralSpeechManager(
        HttpClient httpClient,
        IVoiceManager voiceManager,
        ILogger<MistralSpeechManager> logger,
        string sttModel = "voxtral-mini-latest",
        string ttsModel = "voxtral-mini-tts-latest")
    {
        _httpClient = httpClient;
        _voiceManager = voiceManager;
        _logger = logger;
        _sttModel = sttModel;
        _ttsModel = ttsModel;
        _logger.LogInformation("[Speech:Mistral] STT={SttModel} TTS={TtsModel} BaseUrl={BaseUrl}",
            _sttModel, _ttsModel, httpClient.BaseAddress);
    }

    public async Task<string> TranscribeSpeechAsync(Stream audioStream, Language language, IEnumerable<string> contextBias = null, AudioMimeType mimeType = null)
    {
        mimeType ??= AudioMimeType.Webm;
        try
        {
            using var content = new MultipartFormDataContent();

            using MemoryStream audioMemoryStream = new MemoryStream();
            await audioStream.CopyToAsync(audioMemoryStream);
            byte[] audioBytes = audioMemoryStream.ToArray();
            ByteArrayContent audioContent = new ByteArrayContent(audioBytes);
            
            audioContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType.Value);
            content.Add(audioContent, "file", $"audio.{mimeType.FileExtension}");

            content.Add(new StringContent(_sttModel), "model");
            content.Add(new StringContent(language.ToString()), "language");

            List<string> biasWords = contextBias?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .SelectMany(s => s.Split([' ', ',', '.', '?', '!', ';', ':', '"', '\'', '(', ')'], StringSplitOptions.RemoveEmptyEntries))
                .Where(w => w.Length >= 3)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(100)
                .ToList();

            if (biasWords?.Count > 0)
            {
                foreach (string word in biasWords)
                    content.Add(new StringContent(word), "context_bias");
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "audio/transcriptions");
            request.Content = content;

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Mistral API returned: {response.StatusCode}, Response: {responseContent}");
            }

            MistralTranscriptionResponse result = JsonSerializer.Deserialize<MistralTranscriptionResponse>(responseContent);

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

    public async Task<Stream> SynthesizeSpeechAsync(string text, Language language)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "audio/speech");

            MistralVoice voice = _voiceManager.GetVoiceForLanguage(language);
            var payload = new
            {
                model = _ttsModel,
                input = text,
                voice = voice.Name,
                response_format = "mp3"
            };

            StringContent jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            request.Content = jsonContent;

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseContentText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Mistral TTS API returned: {response.StatusCode}, Response: {responseContentText}");
            }

            MistralTtsResponse result = JsonSerializer.Deserialize<MistralTtsResponse>(responseContentText);
            if (string.IsNullOrEmpty(result?.AudioData))
            {
                throw new HttpRequestException("No audio_data in response from Mistral TTS API");
            }

            byte[] audioBytes = Convert.FromBase64String(result.AudioData);
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
