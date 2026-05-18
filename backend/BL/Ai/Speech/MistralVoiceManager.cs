namespace Conversey.BL.Ai.Speech;

public class MistralVoiceManager : IVoiceManager
{
    private static readonly Dictionary<string, string> Voices = new(StringComparer.OrdinalIgnoreCase)
    {
        { "en", "en_paul_neutral" },
        { "fr", "fr_marie_neutral" },
        { "nl", "en_paul_neutral" }, // no Dutch preset in Mistral; Voxtral speaks Dutch from text
    };

    private const string FallbackVoice = "en_paul_neutral";

    public string GetVoiceForLanguage(string language) =>
        Voices.TryGetValue(language ?? string.Empty, out var voice) ? voice : FallbackVoice;
}
