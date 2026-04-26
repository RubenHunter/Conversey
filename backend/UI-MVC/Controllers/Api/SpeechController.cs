using Conversey.BL.Speech;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

public class SpeechController : Controller
{
    private readonly IMistralSpeechService _speechService;
    private readonly IConfiguration _configuration;

    public SpeechController(IMistralSpeechService speechService, IConfiguration configuration)
    {
        _speechService = speechService;
        _configuration = configuration;
    }

    [HttpPost("transcribe")]
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

    [HttpPost("synthesize")]
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
