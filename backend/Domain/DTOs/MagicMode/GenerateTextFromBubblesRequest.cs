namespace Conversey.BL.Domain.DTOs.MagicMode;

public record GenerateTextFromBubblesRequest(
    string Transcript,
    IReadOnlyList<string> Bubbles,
    string Language);
