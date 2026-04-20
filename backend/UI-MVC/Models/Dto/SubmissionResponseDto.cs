using Conversey.BL.Ai;

namespace Conversey.UI_MVC.Models.Dto;

public abstract record SubmissionResponseDto
{
    public sealed record Approved(IdeaDto idea) : SubmissionResponseDto;

    public sealed record Pending(IdeaDto idea, ModerationDecision decision) : SubmissionResponseDto;
}