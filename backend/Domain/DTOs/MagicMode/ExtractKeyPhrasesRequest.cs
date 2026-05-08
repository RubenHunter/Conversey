namespace Conversey.BL.Domain.DTOs.MagicMode;

public record ExtractKeyPhrasesRequest(
    string Transcript,
    string Language,
    int MaxPhrases = 2,
    IReadOnlyList<string> ExistingPhrases = null,
    IReadOnlyList<string> RejectedPhrases = null);
