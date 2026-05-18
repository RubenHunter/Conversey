namespace Conversey.BL.Ai.DTOs;

public record ExtractKeyPhrasesRequest(
    string Transcript,
    string Language,
    int MaxPhrases = 2,
    IReadOnlyList<string> ExistingPhrases = null,
    IReadOnlyList<string> RejectedPhrases = null);
