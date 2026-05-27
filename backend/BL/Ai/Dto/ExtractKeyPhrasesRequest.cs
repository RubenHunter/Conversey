using System.ComponentModel.DataAnnotations;
using Conversey.BL.Domain.Common;

namespace Conversey.BL.Ai.Dto;

public record ExtractKeyPhrasesRequest(
    [Required] string Transcript,
    Language Language,
    [Range(1, 10)] int MaxPhrases = 2,
    IReadOnlyList<string> ExistingPhrases = null,
    IReadOnlyList<string> RejectedPhrases = null);
