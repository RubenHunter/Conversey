namespace Conversey.BL.Ai.DTOs;

public record GenerateTextFromBubblesRequest(
    string Transcript,
    IReadOnlyList<string> Bubbles,
    string Language,
    IReadOnlyList<string> RejectedPhrases = null);
