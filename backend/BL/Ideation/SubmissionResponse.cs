using Conversey.BL.Ai;
using Conversey.BL.Domain.Ideation;

namespace Conversey.BL.Ideation;

public abstract record SubmissionResponse
{
    public sealed record Approved(Idea idea) : SubmissionResponse;

    public sealed record Pending(Idea idea, ModerationDecision decision) : SubmissionResponse;
}