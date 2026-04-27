using System.IO;

namespace Conversey.BL.Speech;

public interface IMistralSpeechManager
{
    Task<string> TranscribeSpeechAsync(Stream audioStream, string language, IEnumerable<string>? contextBias = null);
    Task<Stream> SynthesizeSpeechAsync(string text, string language);
}
