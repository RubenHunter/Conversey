using Microsoft.Extensions.Logging;

namespace Conversey.BL.Ai.Speech;

public class NoopSpeechManager : ISpeechManager
{
    private readonly ILogger<NoopSpeechManager> _logger;

    public NoopSpeechManager(ILogger<NoopSpeechManager> logger)
    {
        _logger = logger;
        _logger.LogInformation("[Speech:Noop] Speech disabled — all calls return empty");
    }

    public Task<string> TranscribeSpeechAsync(Stream audioStream, string language, IEnumerable<string> contextBias = null, string mimeType = "audio/webm")
    {
        return Task.FromResult(string.Empty);
    }

    public Task<Stream> SynthesizeSpeechAsync(string text, string language)
    {
        return Task.FromResult<Stream>(new MemoryStream());
    }
}
