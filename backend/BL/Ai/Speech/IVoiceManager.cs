using Conversey.BL.Domain.Common;

namespace Conversey.BL.Ai.Speech;

public interface IVoiceManager
{
    MistralVoice GetVoiceForLanguage(Language language);
}