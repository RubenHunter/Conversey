using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Ai.DTOs;

public record GenerateTextFromBubblesRequest(
    [Required] string Transcript,
    [Required, MinLength(1)] IReadOnlyList<string> Bubbles,
    Language Language,
    IReadOnlyList<string> RejectedPhrases = null);
