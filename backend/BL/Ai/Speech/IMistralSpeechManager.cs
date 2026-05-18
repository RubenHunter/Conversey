namespace Conversey.BL.Ai.Speech;

public interface ISpeechManager
{
    Task<string> TranscribeSpeechAsync(Stream audioStream, string language, IEnumerable<string> contextBias = null, string mimeType = "audio/webm");
    Task<Stream> SynthesizeSpeechAsync(string text, string language);
}
