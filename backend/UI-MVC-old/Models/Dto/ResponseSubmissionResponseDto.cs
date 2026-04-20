using Conversey.BL.Ai;

namespace Conversey.UI_MVC.Models.Dto;

public abstract record ResponseSubmissionResponseDto
{
    public sealed record Approved(ResponseDto Response) : ResponseSubmissionResponseDto;

    public sealed record Pending(ResponseDto Response, ModerationDecision decision) : ResponseSubmissionResponseDto;
}

