namespace Conversey.BL.Ai.Dto;

public enum PhraseRejectionReason
{
    None,
    WordCountTooLow,
    WordCountExceeded,
    DuplicateExact,
    DuplicateSemantic,
    SubsetOfExisting,
    FillerContent,
    TooGeneric
}

public record RejectedPhrase(string Phrase, PhraseRejectionReason Reason, string SimilarTo = null);

public record ExtractKeyPhrasesResponse(
    IReadOnlyList<string> Phrases,
    IReadOnlyList<RejectedPhrase> RejectedPhrasesWithReasons = null);
