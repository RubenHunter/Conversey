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

    public MistralSpeechManager(HttpClient httpClient, string apiKey, string sttModel = "voxtral-mini-latest", string ttsModel = "voxtral-mini-tts-latest")
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _sttModel = sttModel;
        _ttsModel = ttsModel;
    }

    public async Task<string> TranscribeSpeechAsync(Stream audioStream, string language, string prompt)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            
            // Add audio file (let Mistral detect the format from the data)
            using var audioMemoryStream = new MemoryStream();
            await audioStream.CopyToAsync(audioMemoryStream);
            var audioContent = new ByteArrayContent(audioMemoryStream.ToArray());
            // Mistral supports: audio/mpeg, audio/wav, audio/ogg, audio/webm, audio/flac, audio/aac, audio/mp4, audio/x-m4a
            content.Add(audioContent, "file", "audio.webm");
            
            // Add model
            content.Add(new StringContent(_sttModel), "model");
            
            // Add optional parameters
            content.Add(new StringContent(language), "language");
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                content.Add(new StringContent(prompt), "prompt");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "audio/transcriptions");
            request.Content = content;

            // Debug: Log audio size
            Console.WriteLine($"[DEBUG] Audio size: {audioMemoryStream.Length} bytes, Model: {_sttModel}, Language: {language}");

            var response = await _httpClient.SendAsync(request);
            
            // Debug logging
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG] Mistral STT Response: {response.StatusCode}");
            Console.WriteLine($"[DEBUG] Response Body: {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Mistral API returned: {response.StatusCode}, Response: {responseContent}");
            }

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
            
            var payload = new
            {
                model = _ttsModel,
                input = text,
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

            // Return the audio stream
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/mp3");
            return await response.Content.ReadAsStreamAsync();
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
        public string Text { get; set; } = string.Empty;
    }
}
