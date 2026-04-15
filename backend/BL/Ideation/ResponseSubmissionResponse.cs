using Conversey.BL.Ai;

namespace Conversey.BL.Ideation;

public abstract record ResponseSubmissionResponse
{
    public sealed record Approved(IdeaResponse Response) : ResponseSubmissionResponse;

    public sealed record Pending(IdeaResponse Response, ModerationDecision decision) : ResponseSubmissionResponse;
}

