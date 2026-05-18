using System.Text.RegularExpressions;
using Conversey.BL.Ai.Speech;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/speech")]
public class SpeechController : ControllerBase
{
    private readonly ISpeechManager _speechManager;
    private readonly ILogger<SpeechController> _logger;

    public SpeechController(ISpeechManager speechManager, ILogger<SpeechController> logger)
    {
        _speechManager = speechManager;
        _logger = logger;
    }

    [HttpPost("transcribe")]
    public async Task<IActionResult> Transcribe([FromBody] SpeechTranscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AudioBase64))
        {
            return BadRequest("Audio data is required.");
        }

        try
        {
            var audioBytes = Convert.FromBase64String(request.AudioBase64);
            using var audioStream = new MemoryStream(audioBytes);

            var language = string.IsNullOrWhiteSpace(request.Language) ? "nl" : request.Language;
            var text = await _speechManager.TranscribeSpeechAsync(audioStream, language, request.ContextBias, request.MimeType);

            return Ok(new { text = text });
        }
        catch (HttpRequestException ex)
        {
            return HandleHttpException(ex);
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException;
            if (inner is HttpRequestException innerHttp)
                return HandleHttpException(innerHttp);

            _logger.LogError(ex, "Speech transcription failed");
            return StatusCode(502, new { error = "Speech transcription unavailable" });
        }
    }

    [HttpPost("synthesize")]
    public async Task<IActionResult> Synthesize([FromBody] TextSynthesizeRequest synthesizeRequest)
    {
        if (string.IsNullOrWhiteSpace(synthesizeRequest.Input))
        {
            return BadRequest("Text is required.");
        }

        try
        {
            var language = string.IsNullOrWhiteSpace(synthesizeRequest.Language) ? "nl" : synthesizeRequest.Language;
            var audioStream = await _speechManager.SynthesizeSpeechAsync(synthesizeRequest.Input, language);

            return File(audioStream, "audio/mp3");
        }
        catch (HttpRequestException ex)
        {
            return HandleHttpException(ex);
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException;
            if (inner is HttpRequestException innerHttp)
                return HandleHttpException(innerHttp);

            _logger.LogError(ex, "Speech synthesis failed");
            return StatusCode(502, new { error = "Speech synthesis unavailable" });
        }
    }

    private IActionResult HandleHttpException(HttpRequestException ex)
    {
        var match = Regex.Match(ex.Message, @"returned:\s*(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var statusCode) &&
            statusCode is 429 or 401 or 403 or 503)
        {
            _logger.LogWarning(ex, "Speech provider returned {StatusCode}", statusCode);
            return StatusCode(statusCode, new { error = ex.Message });
        }

        _logger.LogError(ex, "Speech HTTP error");
        return StatusCode(502, new { error = "Speech service error" });
    }
}