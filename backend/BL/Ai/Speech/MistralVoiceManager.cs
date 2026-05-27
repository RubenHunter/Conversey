using Conversey.BL.Domain.Common;

namespace Conversey.BL.Ai.Speech;

public class MistralVoiceManager : IVoiceManager
{
    private static readonly Dictionary<Language, MistralVoice> Voices = new()
    {
        { Language.En, MistralVoice.EnPaulNeutral },
        { Language.Fr, MistralVoice.FrMarieNeutral },
        { Language.Nl, MistralVoice.EnPaulNeutral }, // no Dutch preset in Mistral; Voxtral speaks Dutch from text
    };

    private static readonly MistralVoice FallbackVoice = MistralVoice.EnPaulNeutral;

    public MistralVoice GetVoiceForLanguage(Language language) =>
        Voices.TryGetValue(language, out MistralVoice voice) ? voice : FallbackVoice;
}