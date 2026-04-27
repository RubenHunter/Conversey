using System.IO;

namespace Conversey.BL.Speech;

public interface IMistralSpeechManager
{
    Task<string> TranscribeSpeechAsync(Stream audioStream, string language, string prompt);
    Task<Stream> SynthesizeSpeechAsync(string text, string language);
}
