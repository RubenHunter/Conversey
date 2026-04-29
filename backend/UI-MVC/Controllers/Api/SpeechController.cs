using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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
    private readonly ILogger<SpeechController> _logger;

    public SpeechController(IMistralSpeechManager speechManager, IConfiguration configuration, ILogger<SpeechController> logger)
    {
        _speechManager = speechManager;
        _configuration = configuration;
        _logger = logger;
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
            var text = await _speechManager.TranscribeSpeechAsync(audioStream, language, request.ContextBias, request.MimeType);
            
            return Ok(new { text = text });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                error = "Fout bij het verwerken van spraak. Probeer het later opnieuw.",
                details = ex.InnerException?.Message ?? ex.Message
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

    [HttpGet("transcribe-stream")]
    public async Task TranscribeStream()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var apiKey = _configuration["AI:Mistral:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsync("API key not configured");
            return;
        }

        var model = HttpContext.Request.Query["model"].FirstOrDefault() ?? "voxtral-mini-transcribe-realtime-2602";
        var language = HttpContext.Request.Query["language"].FirstOrDefault() ?? "nl";

        using var clientWs = await HttpContext.WebSockets.AcceptWebSocketAsync();
        _logger.LogInformation("[STT] Client WebSocket connected");

        using var mistralWs = new ClientWebSocket();
        mistralWs.Options.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        var mistralWsUrl = $"wss://api.mistral.ai/v1/audio/transcriptions?model={Uri.EscapeDataString(model)}&language={Uri.EscapeDataString(language)}";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            await mistralWs.ConnectAsync(new Uri(mistralWsUrl), cts.Token);
            _logger.LogInformation("[STT] Connected to Mistral WebSocket");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[STT] Failed to connect to Mistral WebSocket");
            await clientWs.CloseAsync(WebSocketCloseStatus.InternalServerError, "Mistral connection failed", CancellationToken.None);
            return;
        }

        var startBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { type = "start", model, language }));
        await mistralWs.SendAsync(new ArraySegment<byte>(startBytes), WebSocketMessageType.Text, true, CancellationToken.None);

        var clientToMistral = Task.Run(async () =>
        {
            var buffer = new byte[65536];
            try
            {
                while (clientWs.State == WebSocketState.Open && mistralWs.State == WebSocketState.Open)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await clientWs.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.Count > 0) ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await mistralWs.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                        break;
                    }
                    await mistralWs.SendAsync(ms.ToArray(), result.MessageType, true, CancellationToken.None);
                }
            }
            catch
            {
                if (mistralWs.State == WebSocketState.Open)
                    await mistralWs.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error", CancellationToken.None);
            }
        });

        var mistralToClient = Task.Run(async () =>
        {
            var buffer = new byte[65536];
            try
            {
                while (clientWs.State == WebSocketState.Open && mistralWs.State == WebSocketState.Open)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await mistralWs.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.Count > 0) ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await clientWs.CloseAsync(WebSocketCloseStatus.NormalClosure, "Mistral closed", CancellationToken.None);
                        break;
                    }
                    await clientWs.SendAsync(ms.ToArray(), result.MessageType, true, CancellationToken.None);
                }
            }
            catch
            {
                if (clientWs.State == WebSocketState.Open)
                    await clientWs.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error", CancellationToken.None);
            }
        });

        await Task.WhenAll(clientToMistral, mistralToClient);
    }
}
