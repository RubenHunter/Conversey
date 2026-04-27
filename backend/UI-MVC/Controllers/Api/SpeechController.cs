using Conversey.BL.Speech;
using Conversey.UI_MVC.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Conversey.UI_MVC.Controllers.Api;

[ApiController]
[Route("api/speech")]
public class SpeechController : Controller
{
    private readonly IMistralSpeechManager _speechManager;
    private readonly IConfiguration _configuration;

    public SpeechController(IMistralSpeechManager speechManager, IConfiguration configuration)
    {
        _speechManager = speechManager;
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
            var text = await _speechManager.TranscribeSpeechAsync(audioStream, language, request.ContextBias);
            
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
        if (string.IsNullOrWhiteSpace(request.Input))
        {
            return BadRequest("Text is required.");
        }

        try
        {
            var language = string.IsNullOrWhiteSpace(request.Language) ? "nl" : request.Language;
            var audioStream = await _speechManager.SynthesizeSpeechAsync(request.Input, language);
            
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
