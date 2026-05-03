namespace UI_MVC.DTOs.MagicMode;

public record ExtractKeyPhrasesRequest(
    string Transcript,
    string Language,
    int MaxPhrases = 5,
    IReadOnlyList<string> ExistingPhrases = null,
    IReadOnlyList<string> RejectedPhrases = null);
