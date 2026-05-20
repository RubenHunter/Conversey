using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ai.Speech;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Conversey.BL.Ai.Speech;

public class NoopSpeechManager : ISpeechManager
{
    private readonly ILogger<NoopSpeechManager> _logger;

    public NoopSpeechManager(ILogger<NoopSpeechManager> logger)
    {
        _logger = logger;
    }

    public Task<string> TranscribeSpeechAsync(Stream audioStream, Language language, IEnumerable<string> contextBias = null, AudioMimeType mimeType = null)
    {
        _logger.LogInformation("[NoopSpeech] Fake transcribe requested ({Lang})", language);
        return Task.FromResult("This is a simulated transcription from NoopSpeechManager.");
    }

    public Task<Stream> SynthesizeSpeechAsync(string text, Language language)
    {
        _logger.LogInformation("[NoopSpeech] Fake synthesize requested ({Lang}, length {Len})", language, text.Length);
        var bytes = Encoding.UTF8.GetBytes("Fake MP3 Data");
        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }
}