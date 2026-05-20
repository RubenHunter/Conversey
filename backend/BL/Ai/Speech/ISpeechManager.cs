using Conversey.BL.Domain.Common;
using Conversey.BL.Domain.Ai.Speech;

namespace Conversey.BL.Ai.Speech;

public interface ISpeechManager
{
    Task<string> TranscribeSpeechAsync(Stream audioStream, Language language, IEnumerable<string> contextBias = null, AudioMimeType mimeType = null);
    Task<Stream> SynthesizeSpeechAsync(string text, Language language);
}
