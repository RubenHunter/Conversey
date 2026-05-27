using Conversey.BL.Ai;

namespace Conversey.BL.Ideation;

public abstract record ResponseSubmissionResponse
{
    public sealed record Approved(Conversey.BL.Domain.Ideation.IdeaResponse IdeaResponse) : ResponseSubmissionResponse;

    public sealed record Pending(Conversey.BL.Domain.Ideation.IdeaResponse IdeaResponse, ModerationDecision Decision) : ResponseSubmissionResponse;
}

