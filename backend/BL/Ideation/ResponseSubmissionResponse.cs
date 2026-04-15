using Conversey.BL.Ai;

namespace Conversey.BL.Ideation;

public abstract record ResponseSubmissionResponse
{
    public sealed record Approved(Conversey.BL.Domain.Ideation.Response Response) : ResponseSubmissionResponse;

    public sealed record Pending(Conversey.BL.Domain.Ideation.Response Response, ModerationDecision decision) : ResponseSubmissionResponse;
}

