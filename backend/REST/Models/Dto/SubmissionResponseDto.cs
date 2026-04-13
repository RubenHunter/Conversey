using Conversey.BL.Ai;
using Conversey.BL.Domain.Subplatform.Survey.Ideation;

namespace Conversey.REST.Models.Dto;

public abstract record SubmissionResponseDto
{
    public sealed record Approved(IdeaDto idea) : SubmissionResponseDto;

    public sealed record Pending(IdeaDto idea, ModerationDecision decision) : SubmissionResponseDto;
}