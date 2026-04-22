using Conversey.BL.Speech;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/speech")]
public class SpeechController : Controller
{
    private readonly IMistralSpeechService _speechService;
    private readonly IConfiguration _configuration;

    public SpeechController(IMistralSpeechService speechService, IConfiguration configuration)
    {
        _speechService = speechService;
        _configuration = configuration;
    }

    /// <summary>
    /// Converteert spraak (audio) naar tekst met Voxtral Mini Transcribe.
    /// </summary>
    /// <param name="audioBase64">Audio in base64-formaat.</param>
    /// <param name="language">Taalcode (nl, en, fr). Default: nl.</param>
    /// <returns>De herkende tekst.</returns>
    [HttpPost("transcribe")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<string>> Transcribe([FromBody] TranscribeRequest request)
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
            var text = await _speechService.TranscribeSpeechAsync(audioStream, language, request.Prompt);
            
            return Ok(new { text = text });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                Error = "Fout bij het verwerken van spraak. Probeer het later opnieuw.",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Converteert tekst naar spraak met Voxtral TTS.
    /// </summary>
    /// <param name="request">Request met tekst en taal.</param>
    /// <returns>Audio stream (MP3).</returns>



    [HttpPost("synthesize")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Synthesize([FromBody] SynthesizeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text is required.");
        }

        try
        {
            var language = string.IsNullOrWhiteSpace(request.Language) ? "nl" : request.Language;
            var audioStream = await _speechService.SynthesizeSpeechAsync(request.Text, language);
            
            return File(audioStream, "audio/mp3");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                Error = "Fout bij het genereren van spraak. Probeer het later opnieuw.",
                Details = ex.Message
            });
        }
    }
}

public class TranscribeRequest
{
    public string AudioBase64 { get; set; } = string.Empty;
    public string? Language { get; set; }
    public string? Prompt { get; set; }
}

public class SynthesizeRequest
{
    public string Text { get; set; } = string.Empty;
    public string? Language { get; set; }
}
